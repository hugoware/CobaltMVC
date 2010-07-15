using System;
using System.Collections.Generic;
using System.Linq;
using Cobalt.Html;

namespace Cobalt.Web {

    /// <summary>
    /// Handles holding commands that should be executed for
    /// specific control or page context
    /// </summary>
    public class ReadyContext {

        #region Constructors

        /// <summary>
        /// Handles creating a new context for commands to work in
        /// </summary>
        public ReadyContext() {
            this._OnReadyEvents = new Action[] { };
            this._OnConstructEvents = new Action[] { };
            this._OnFinalizeEvents = new Action[] { };
            this.Nodes = new HtmlNode[] { };
        }

        #endregion

        #region Properties

        /// <summary>
        /// The markup identity for this control (for rebuilding the page)
        /// </summary>
        public string Identity { get; private set; }

        /// <summary>
        /// The Control or Page context this belongs to
        /// </summary>
        public object Context { get; private set; }

        /// <summary>
        /// The nodes that make up this ready context
        /// </summary>
        public IEnumerable<HtmlNode> Nodes { get; private set; }

        //actions to execute
        private IEnumerable<Action> _OnReadyEvents;
        private IEnumerable<Action> _OnConstructEvents;
        private IEnumerable<Action> _OnFinalizeEvents;

        #endregion

        #region Methods

        /// <summary>
        /// Applies all waiting commands for this context
        /// </summary>
        public void Execute() {

            //prepare the construction
            foreach (Action action in this._OnConstructEvents.ToArray()) {
                action();
            }

            //perform the work
            foreach (Action action in this._OnReadyEvents.ToArray()) {
                action();
            }

            //finalize the changes
            foreach (Action action in this._OnFinalizeEvents.ToArray()) {
                action();
            }

            //remove the actions entirely
            this._OnReadyEvents = new Action[] { };
            this._OnFinalizeEvents = new Action[] { };
            this._OnConstructEvents = new Action[] { };
        }

        /// <summary>
        /// Appends additional actions to this context
        /// </summary>
        public void AddReadyEvent(Action action) {
            this._OnReadyEvents = this._OnReadyEvents.Union(new Action[] { action }).Distinct();
        }

        /// <summary>
        /// Appends additional events to invoke before processing an element
        /// </summary>
        public void AddConstructEvent(Action action) {
            this._OnConstructEvents = this._OnConstructEvents.Union(new Action[] { action }).Distinct();
        }

        /// <summary>
        /// Appends additional events to invoke when finishing an element
        /// </summary>
        public void AddFinalizeEvent(Action action) {
            this._OnFinalizeEvents = this._OnFinalizeEvents.Union(new Action[] { action }).Distinct();
        }

        /// <summary>
        /// Append an additional node to this context
        /// </summary>
        public void AddNodes(HtmlNode node) {
            this.AddNodes(new HtmlNode[] { node });
        }

        /// <summary>
        /// Appends additional nodes to this context
        /// </summary>
        public void AddNodes(IEnumerable<HtmlNode> nodes) {
            this.Nodes = this.Nodes.Union(nodes).Distinct();
        }

        /// <summary>
        /// Updates the context for this element
        /// </summary>
        public void SetContext(object context) {
            this.Context = context;
        }

        /// <summary>
        /// Updates the identity for this ready context (if needed)
        /// </summary>
        public void SetIdentity(string identity) {
            this.Identity = identity;
        }

        /// <summary>
        /// Determines if two contexts are equal or not
        /// </summary>
        public bool IsContextFor(object context) {
            return this.Context != null
                ? this.Context.Equals(context)
                : context == null;
        }

        /// <summary>
        /// Determines if two identity are equal or not
        /// </summary>
        public bool IsIdentityFor(string identity) {
            return this.Identity is string
                ? this.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase)
                : identity == null;
        }

        /// <summary>
        /// Checks if a set of actions should be processed or not
        /// </summary>
        public bool ShouldBeProcessed() {
            return this.Nodes != null && this.Nodes.Any();
        }

        #endregion

    }

}
