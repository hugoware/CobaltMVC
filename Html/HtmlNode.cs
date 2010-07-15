using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using Cobalt.Web;

namespace Cobalt.Html {

    /// <summary>
    /// Html content found within a page
    /// </summary>
    public class HtmlNode {

        #region Constants

        /// <summary>
        /// Special suffix on certain Html tags to prevent errors
        /// </summary>
        internal const string COBALT_ELEMENT_TAG_HELPER = "_protectedelement";

        //holds tags that need to be fixed before using
        private static readonly Regex HtmlAgilityPackFixTags = new Regex("^(" + CobaltContext.PROTECTED_TAGS + ")$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new HtmlNode from the content provided
        /// </summary>
        internal HtmlNode(HtmlAgilityPack.HtmlNode node) {
            this._Container = node ?? HtmlAgilityPack.HtmlNode.CreateNode(" ");
        }

        #endregion

        #region Properties

        //temporary container until HTML parser can be written
        internal HtmlAgilityPack.HtmlNode _Container;

        /// <summary>
        /// Gets or sets attribute values for this node
        /// </summary>
        public virtual object this[string attribute] {
            get {

                //check for format
                attribute = (attribute ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(attribute)) { return string.Empty; }

                //return the value if any
                return this._Container.Attributes[attribute] is HtmlAgilityPack.HtmlAttribute
                    ? this._Container.Attributes[attribute].Value
                    : string.Empty;
            }
            set {

                //check for format
                attribute = (attribute ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(attribute)) { return; }

                //clear the current value
                this._Container.Attributes.Remove(attribute);
                if (value != null) {
                    this._Container.Attributes.Add(attribute, value.ToString());
                }

            }

        }

        /// <summary>
        /// Gets or sets the tag for this element
        /// </summary>
        public string Tag {
            get { return (this._Container.Name ?? string.Empty).Trim(); }
            set { this._Container.Name = value; }
        }

        /// <summary>
        /// Holds the actual name of the tag, even when processing
        /// </summary>
        internal string SelectorTag {
            get { return this._Container.Name; }
        }

        /// <summary>
        /// Information about where this node is
        /// </summary>
        public string Path {
            get { return this._Container.XPath; }
        }

        /// <summary>
        /// Returns the owner node for this element
        /// </summary>
        public virtual HtmlNode Parent {
            get { return new HtmlNode(this._Container.ParentNode); }
        }

        /// <summary>
        /// Returns a list of the current child nodes
        /// </summary>
        public virtual IEnumerable<HtmlNode> Children {
            get { return this._Container.ChildNodes.Select(node => new HtmlNode(node)); }
        }

        /// <summary>
        /// Returns a list of the current child nodes
        /// </summary>
        public virtual IEnumerable<HtmlNode> Siblings {
            get {
                //make sure there is a parent to work with
                if (this.Parent == null) { return new HtmlNode[] { }; }
                
                //get the list to hold the items
                List<HtmlAgilityPack.HtmlNode> siblings = new List<HtmlAgilityPack.HtmlNode>();
                siblings.Add(this._Container);

                //loop though the previous nodes
                HtmlAgilityPack.HtmlNode current = this._Container;
                while(current != null) {

                    //try and select it
                    current = this._Container.PreviousSibling;
                    if (current != null) {
                        siblings.Insert(0, current);
                    }
                }

                //grab the other direction of nodes
                current = this._Container;
                while (current != null) {

                    //try and select it
                    current = this._Container.NextSibling;
                    if (current != null) {
                        siblings.Add(current);
                    }
                }

                //finally, return the nodes to use
                return siblings.Select(node => new HtmlNode(node));

            }
        }

        /// <summary>
        /// Gets or sets the value of this node as html escaped
        /// </summary>
        public virtual string InnerText {
            get { return this._Container.InnerText; }
            set { this._Container.InnerHtml = HttpUtility.HtmlEncode(value); }
        }

        /// <summary>
        /// Gets or sets the value of this node
        /// </summary>
        public virtual string InnerHtml {
            get { return this._Container.InnerHtml; }
            set { this._Container.InnerHtml = value; }
        }

        /// <summary>
        /// Is this an html element with a tag
        /// </summary>
        public bool IsElement {
            get { return this._Container.NodeType == HtmlAgilityPack.HtmlNodeType.Element; }
        }

        /// <summary>
        /// Is this an HTML comment
        /// </summary>
        public bool IsComment {
            get { return this._Container.NodeType == HtmlAgilityPack.HtmlNodeType.Comment; }
        }

        /// <summary>
        /// Is this the root of the document
        /// </summary>
        public bool IsDocument {
            get { return this._Container.NodeType == HtmlAgilityPack.HtmlNodeType.Document; }
        }

        /// <summary>
        /// Is this text content (may contain nodes)
        /// </summary>
        public bool IsText {
            get { return this._Container.NodeType == HtmlAgilityPack.HtmlNodeType.Text; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the value for this element
        /// </summary>
        public string GetValue() {
            return this.IsInput() ? this["value"] as string
                : this.IsTextArea() ? this.InnerHtml
                : string.Empty;
        }

        /// <summary>
        /// Gets the value for this element
        /// </summary>
        public void SetValue(object value) {
            if (this.IsTextArea()) {
                this.InnerHtml = (value ?? string.Empty).ToString();
            }
            else if (this.IsInput()) {
                this["value"] = value;
            }
        }

        /// <summary>
        /// Returns all child nodes and the current node
        /// </summary>
        public virtual IEnumerable<HtmlNode> DescendantsAndSelf() {
            return this._Container.DescendantsAndSelf().Select(node => new HtmlNode(node));
        }

        /// <summary>
        /// Returns all child nodes and the current node
        /// </summary>
        public virtual IEnumerable<HtmlNode> Descendants() {
            return this._Container.Descendants().Select(node => new HtmlNode(node));
        }

        /// <summary>
        /// Removes all attributes from this element
        /// </summary>
        public void ClearAttributes() {
            for (int i = this._Container.Attributes.Count; i-- > 0; ) {
                this._Container.Attributes.RemoveAt(i);
            }
        }

        /// <summary>
        /// Appends each of the CSS class values added
        /// </summary>
        public void AddCss(params string[] css) {
            css = Regex.Split(string.Concat(css), @"\s");
            IEnumerable<string> values = this._GetAttributeValues("class").Union(css).Distinct();
            this._SetAttributeValues("class", values);
        }

        /// <summary>
        /// Removes each of the CSS values from this attribute
        /// </summary>
        public void RemoveCss(params string[] css) {
            css = Regex.Split(string.Concat(css), @"\s");
            IEnumerable<string> values = this._GetAttributeValues("class").Except(css).Distinct();
            this._SetAttributeValues("class", values);
        }

        /// <summary>
        /// Returns all of the CSS classes assigned to a node
        /// </summary>
        public IEnumerable<string> GetCss() {
            return this._GetAttributeValues("class");
        }

        /// <summary>
        /// Checks if an attribute exists or not
        /// </summary>
        public bool HasAttribute(string name) {
            name = (name ?? string.Empty).Trim();
            return this._Container.Attributes[name] != null;
        }

        #endregion

        #region Static Creation

        /// <summary>
        /// Returns an empty html container
        /// </summary>
        public static HtmlNode EmptyNode() {
            return HtmlNode.Parse("").FirstOrDefault();
        }

        /// <summary>
        /// Parses a collection
        /// </summary>
        public static IEnumerable<HtmlNode> Parse(string html) {

            //apply the node name hacks for all parsing
            html = CobaltContext.Current.ApplyHtmlAgilityTextHacks(html);

            //enclose in a container to help HtmlAgility not freak out :)
            html = string.Concat("<container>", html, "</container>");
            HtmlAgilityPack.HtmlNode container = HtmlAgilityPack.HtmlNode.CreateNode(html);

            //return the matching nodes
            return container.ChildNodes.Select(node => new HtmlNode(node));
        }

        #endregion

        #region Helper Methods

        //gets a collection of values from an attribute
        private string[] _GetAttributeValues(string name) {
            return Regex.Split(
                this[name] as string ?? string.Empty, 
                @"\s"
                );
        }

        //sets a collection of values to an attribute
        private void _SetAttributeValues(string name, IEnumerable<string> values) {
            this[name] = values is IEnumerable<string>
                ? string.Join(" ", values.ToArray())
                : null;
        }

        #endregion

        #region Overriding Methods

        /// <summary>
        /// Returns the current markup for this element
        /// </summary>
        public override string ToString() {
            return this._Container.OuterHtml;
        }

        #endregion

        #region Bug Fixes

        /// <summary>
        /// Gets the helper name for this tag if needed
        /// </summary>
        internal static string GetCobaltHelperTagName(string tag) {
            tag = (tag ?? string.Empty).Trim();
            return HtmlNode.RequiresCobaltHelper(tag)
                ? string.Concat(tag, COBALT_ELEMENT_TAG_HELPER)
                : tag;
        }

        /// <summary>
        /// Does this tag need to be protected from HtmlAgilityPack
        /// </summary>
        internal static bool RequiresCobaltHelper(string tag) {
            return HtmlNode.HtmlAgilityPackFixTags.IsMatch(tag);
        }

        #endregion

        #region Manipulation

        /// <summary>
        /// Removes a node entirely
        /// </summary>
        /// <returns></returns>
        public void Remove() {
            this._Container.Remove();
        }

        /// <summary>
        /// Clones the current Html node
        /// </summary>
        public HtmlNode Clone() {
            return new HtmlNode(this._Container.Clone());
        }

        /// <summary>
        /// Moves a node before the target node
        /// </summary>
        public void InsertBefore(HtmlNode target) {
            if (target == null) { return; }
            this.Detatch();
            target.Parent._Container.InsertBefore(
                this._Container,
                target._Container
                );
        }

        /// <summary>
        /// Moves a node after the target node
        /// </summary>
        public void InsertAfter(HtmlNode target) {
            if (target == null) { return; }
            this.Detatch();
            target.Parent._Container.InsertAfter(
                this._Container,
                target._Container
                );
        }

        /// <summary>
        /// Appends a node to this node
        /// </summary>
        public void Append(HtmlNode node) {
            this.Append(new HtmlNode[] { node });
        }

        /// <summary>
        /// Detatches a node from the document (but doesn't delete it)
        /// </summary>
        public void Detatch() {
            HtmlAgilityPack.HtmlNode clone = this._Container.Clone();
            this._Container.Remove();
            this._Container = clone;
        }

        /// <summary>
        /// Appends a set of nodes to this node
        /// </summary>
        public void Append(IEnumerable<HtmlNode> nodes) {
            nodes.Each(node => {
                HtmlAgilityPack.HtmlNode clone = node._Container.Clone();
                node._Container.Remove();
                node._Container = clone;
                this._Container.AppendChild(node._Container);
            });
        }

        /// <summary>
        /// Prepends a node to this node
        /// </summary>
        public void Prepend(HtmlNode node) {
            this.Prepend(new HtmlNode[] { node });
        }

        /// <summary>
        /// Prepends a set of nodes to this node
        /// </summary>
        public void Prepend(IEnumerable<HtmlNode> nodes) {
            nodes.Each(node => {
                HtmlAgilityPack.HtmlNode clone = node._Container.Clone();
                node._Container.Remove();
                node._Container = clone;
                this._Container.PrependChild(node._Container);
            });
        }

        #endregion

    }

}
