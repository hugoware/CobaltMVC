using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using Cobalt.Html;

namespace Cobalt.Web {

    /// <summary>
    /// Contains information about the Cobalt information
    /// being used on the page
    /// </summary>
    public class CobaltContext {

        #region Constants

        //shared tags for fixing HtmlAgility errors
        internal const string PROTECTED_TAGS = "form|p";
        internal const string SELF_CLOSING_TAGS = "input|link|br|hr";

        //other expressions
        private const string CLOSE_NO_CONTENT_TAG = "/>";
        private const string CLOSE_TAG = "</";
        private const string BASIC_TAG = "<";
        private const string GROUP_TAG_NAME = "tag";
        private const string PROTECTED_ELEMENTS_EXPRESSION = "?(?<" + GROUP_TAG_NAME + ">(" + PROTECTED_TAGS + "))";
        private const string NO_CONTENT_TAG_EXPRESSION = "<(" + SELF_CLOSING_TAGS + ")[^>]*>";
        private const string ITEM_COBALT_CONTEXT = "Cobalt:CurrentContext";
        private static readonly Regex _ApplyHtmlAgilityFormHackExpression = new Regex(CLOSE_TAG + PROTECTED_ELEMENTS_EXPRESSION, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _RevertHtmlAgilityFormHackExpression = new Regex(CLOSE_TAG + PROTECTED_ELEMENTS_EXPRESSION + HtmlNode.COBALT_ELEMENT_TAG_HELPER, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _CloseNoContentTags = new Regex(NO_CONTENT_TAG_EXPRESSION, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _IsCloseTag = new Regex(CLOSE_TAG, RegexOptions.Compiled);
        private static readonly Regex _EndsWithCloseTag = new Regex(CLOSE_NO_CONTENT_TAG, RegexOptions.Compiled);

        #endregion

        #region Constructors

        /// <summary>
        /// Holds ready actions for this context
        /// </summary>
        public CobaltContext() {
            this.Phase = CobaltRenderPhase.Waiting;
            this._ReadyActions = new List<ReadyContext>();
            this.Process = true;
        }

        #endregion

        #region Events

        /// <summary>
        /// Delegate used to modify markup before processing
        /// </summary>
        public delegate void CobaltProcessMarkupHandler(ref string content);

        /// <summary>
        /// Raised just before the page markup is processed
        /// </summary>
        public event CobaltProcessMarkupHandler BeforeGenerate;


        /// <summary>
        /// Raised just after the page is generated
        /// </summary>
        public event Action<HtmlDocument> AfterGenerate;

        /// <summary>
        /// Raised after any hacks are applied and before Ready events are processed
        /// </summary>
        public event Action<HtmlDocument> BeforeApply;

        /// <summary>
        /// Raised after all Ready events are processed
        /// </summary>
        public event Action<HtmlDocument> AfterApply;

        /// <summary>
        /// Raised just before the final markup is generated
        /// </summary>
        public event Action<HtmlDocument> BeforeFinalize;

        /// <summary>
        /// Raised just after the document has been converted to a string
        /// </summary>
        public event CobaltProcessMarkupHandler AfterFinalize;

        #endregion

        #region Static Properties

        /// <summary>
        /// Holds the current context being worked in
        /// </summary>
        public static CobaltContext Current {
            get {

                //check if there is a context waiting
                CobaltContext context = HttpContext.Current.Items[ITEM_COBALT_CONTEXT] as CobaltContext;
                if (context == null) {

                    //create a new one if needed
                    context = new CobaltContext();
                    HttpContext.Current.Items[ITEM_COBALT_CONTEXT] = context;
                }

                //return the context to use
                return context;

            }
        }

        /// <summary>
        /// The context that is currently rendering
        /// </summary>
        public object Rendering {
            get { return this._Rendering; }
        }
        private object _Rendering;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current phase of the CobaltContext
        /// </summary>
        public CobaltRenderPhase Phase { get; private set; }

        /// <summary>
        /// Returns an instance of the executing page
        /// </summary>
        public CobaltElement Document { get; internal set; }

        /// <summary>
        /// Returns an instance to the Page being rendered
        /// </summary>
        public Page Page { get; internal set; }

        //holds if Cobalt should process this page or not
        internal bool Process { get; private set; }

        //holds a list of actions 
        private List<ReadyContext> _ReadyActions;

        //holds the currently processed document
        internal HtmlDocument DocumentHtml;

        #endregion

        #region Methods

        /// <summary>
        /// This cancels all Cobalt processing if calledbefore the Generate phase
        /// </summary>
        public void CancelProcessing() {
            this.Process = false;
        }

        /// <summary>
        /// Handles applying all waiting actions
        /// </summary>
        internal string ProcessDocument(object context, string html) {


            //just in case, cancel this step if not processing
            this.Page = context as Page;
            if (!this.Process || this.Phase == CobaltRenderPhase.Completed) { return html; }
            Log.Write(this, "Preparing page");

            //creates the document content
            if (this.BeforeGenerate != null) { this.BeforeGenerate(ref html); }
            this.Phase = CobaltRenderPhase.Generate;
            this._GenerateDocumentPageContext(context, html);
            Log.Write(this, "Created page context");

            //rebuild the content
            this._PopulateHtmlContextNodes();
            Log.Write(this, "Populated UserControl nodes");
            if (this.AfterGenerate != null) { this.AfterGenerate(this.DocumentHtml); }

            //add any fixes for the document
            if (this.BeforeApply != null) { this.BeforeApply(this.DocumentHtml); }
            this._ApplyHtmlAgilityHacks();
            Log.Write(this, "Applied HTML Agility Hacks");

            //apply the waiting actions
            this.Phase = CobaltRenderPhase.Apply;
            this._ApplyReadyActions();
            Log.Write(this, "Finished ready actions");

            //revert any Html fixes
            this._RevertHtmlAgilityHacks();
            Log.Write(this, "Reverted hacks");
            if (this.AfterApply != null) { this.AfterApply(this.DocumentHtml); }

            //finally, return the finished document
            if (this.BeforeFinalize != null) { this.BeforeFinalize(this.DocumentHtml); }
            this.Phase = CobaltRenderPhase.Rendering;
            string content = this._GetFinalDocumentHtml();
            Log.Write(this, "Exported HTML content");
            this.DocumentHtml = null;
            if (this.AfterFinalize != null) { this.AfterFinalize(ref content); }

            //return the content to write
            this.Phase = CobaltRenderPhase.Completed;
            return content;

        }

        //updates the document and page references
        private void _GenerateDocumentPageContext(object context, string html) {

            //perform any preprocessing for HtmlAgility errors
            html = this.ApplyHtmlAgilityTextHacks(html);
            
            //create the document
            this.DocumentHtml = new HtmlDocument(html);
            this.Document = new CobaltElement(this.DocumentHtml.Root);

            //then register the context
            ReadyContext ready = this.FindReadyContextByInstance(context);
            ready.AddNodes(this.DocumentHtml.Root);
            ready.SetContext(context);

        }

        #endregion

        #region Registering Events

        /// <summary>
        /// Registers an action and the context to run within
        /// </summary>
        internal void RegisterReadyAction(object context, Action action) {
            ReadyContext ready = this.FindReadyContextByInstance(context);
            ready.AddReadyEvent(action);
        }

        /// <summary>
        /// Adds an event to use for constructing controls before processing them
        /// </summary>
        internal void RegisterConstructEvent(object context, Action action) {
            ReadyContext ready = this.FindReadyContextByInstance(context);
            ready.AddConstructEvent(action);
        }

        /// <summary>
        /// Adds an event to use for finalizing controls before rendering them
        /// </summary>
        internal void RegisterFinalizeEvent(object context, Action action) {
            ReadyContext ready = this.FindReadyContextByInstance(context);
            ready.AddFinalizeEvent(action);
        }

        /// <summary>
        /// Registers a html node to point back to a particular context
        /// </summary>
        internal void RegisterControlContext(object context, string identity) {
            ReadyContext ready = this.FindReadyContextByInstance(context);
            ready.SetIdentity(identity);
        }

        /// <summary>
        /// Registers html elements for a particular context
        /// </summary>
        internal void RegisterHtmlContext(string identity, IEnumerable<HtmlNode> nodes) {
            ReadyContext ready = this.FindReadyContextByInstance(identity);
            ready.AddNodes(nodes);
        }

        /// <summary>
        /// Finds the ready context or creates one if nothing exists
        /// </summary>
        internal ReadyContext FindReadyContextByInstance(object context) {
            ReadyContext ready = this._FindReadyContext(match => match.IsContextFor(context));
            if (ready.Context == null) { ready.SetContext(context); }
            return ready;
        }

        /// <summary>
        /// Finds the ready context or creates one if nothing exists
        /// </summary>
        internal ReadyContext FindReadyContextByIdentity(string identity) {
            return this._FindReadyContext(match => match.IsIdentityFor(identity));
        }

        //finds or creates a new context
        private ReadyContext _FindReadyContext(Func<ReadyContext, bool> find) {

            //check for the existing context
            ReadyContext ready;
            ready = this._ReadyActions.FirstOrDefault(find);

            //if it wasn't found, create it
            if (ready == null) {
                ready = new ReadyContext();
                this._ReadyActions.Add(ready);
            }

            //then send it back to be used
            return ready;
        }

        #endregion

        #region Processing Documents

        //posts the correct html nodes for each context
        private void _PopulateHtmlContextNodes() {

            //find all of the placeholders
            HtmlNode[] containers = this.DocumentHtml.Root.DescendantsAndSelf()
                .Where(node => node.Tag.Equals(CobaltConfiguration.CONTROL_CONTEXT_ELEMENT))
                .ToArray();

            //update each container
            for(int i = containers.Length; i --> 0;) {
                HtmlNode container = containers[i];
                
                //find the correct context
                string identity = container["id"] as string;
                ReadyContext context = this.FindReadyContextByIdentity(identity);
                
                //move this out of the container
                var children = container.Children.ToArray();
                foreach (HtmlNode child in children) {
                    child.InsertBefore(container);
                }

                //update the context
                context.AddNodes(children);

                //drop the container
                container.Remove();
            }

        }

        /// <summary>
        /// Applies HtmlAgilityPack hacks to html
        /// </summary>
        internal string ApplyHtmlAgilityTextHacks(string html) {
            return CobaltContext._ApplyHtmlAgilityFormHackExpression.Replace(
                html,
                match => CobaltContext._IsCloseTag.IsMatch(match.Value) 
                    ? string.Concat(CLOSE_TAG, match.Groups[GROUP_TAG_NAME].Value, HtmlNode.COBALT_ELEMENT_TAG_HELPER)
                    : string.Concat(BASIC_TAG, match.Groups[GROUP_TAG_NAME].Value, HtmlNode.COBALT_ELEMENT_TAG_HELPER)
                    );
        }

        /// <summary>
        /// Removes any HtmlAgilityPack hacks from html
        /// </summary>
        internal string RevertHtmlAgilityTextHacks(string html) {

            //fix any tags that shouldn't close incorrectly
            html = CobaltContext._RevertHtmlAgilityFormHackExpression.Replace(
                html,
                match => CobaltContext._IsCloseTag.IsMatch(match.Value)
                    ? string.Concat(CLOSE_TAG, match.Groups[GROUP_TAG_NAME].Value)
                    : string.Concat(BASIC_TAG, match.Groups[GROUP_TAG_NAME].Value)
                    );

            //make sure no content tags are closed correctly
            html = CobaltContext._CloseNoContentTags.Replace(
                html,
                match => CobaltContext._EndsWithCloseTag.IsMatch(match.Value)
                    ? match.Value
                    : string.Concat(match.Value.Substring(0, match.Value.Length - 1), CLOSE_NO_CONTENT_TAG)
                    );

            //return the final markup
            return html;
        }

        //reverts any HtmlAgility hacks
        private string _GetFinalDocumentHtml() {
            string html = this.RevertHtmlAgilityTextHacks(this.Document.ToString());
            return html;
        }

        //makes changes to the document for 
        private void _ApplyHtmlAgilityHacks() {
        }

        //reverts any hacks for the document
        private void _RevertHtmlAgilityHacks() {
        }

        //processes each waiting action
        private void _ApplyReadyActions() {

            ReadyContext[] actions = this._ReadyActions.Where(ready => ready.ShouldBeProcessed()).ToArray();
            for(int i = actions.Length; i --> 0;) {
                ReadyContext action = actions[i];

                //set this as the rendering content
                this._Rendering = action.Context;

                //update and run the events
                Log.Write(action, "Starting {0}...", action.Context); 
                action.Execute();
                Log.Write(action, "Ending {0}...", action.Context);

                //remove this item from rendering
                this._Rendering = null;

            }
            
        }

        #endregion

    }

}
