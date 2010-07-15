using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cobalt.Html;
using Cobalt.Interfaces;
using Cobalt.Css;
using Cobalt.Web;

namespace Cobalt {

    /// <summary>
    /// Contains functionality for working with sets of elements
    /// </summary>
    public class CobaltElement : ICobaltElement {

        #region Constructors

        /// <summary>
        /// Creates an empty CobaltElement
        /// </summary>
        public CobaltElement()
            : this (new HtmlNode[] { }) {
        }

        /// <summary>
        /// Creates a new CobaltElement using the node provided
        /// </summary>
        public CobaltElement(ICobaltElement element)
            : this(element.AsElement()) {
        }

        /// <summary>
        /// Creates a new CobaltElement using the node provided
        /// </summary>
        public CobaltElement(IEnumerable<ICobaltElement> elements)
            : this(elements.SelectMany(element => element.AsElement().Selected)) {
        }

        /// <summary>
        /// Creates a new CobaltElement using the node provided
        /// </summary>
        public CobaltElement(CobaltElement element)
            : this(element.Selected) {
        }

        /// <summary>
        /// Creates a new CobaltElement using the node provided
        /// </summary>
        public CobaltElement(IEnumerable<CobaltElement> elements)
            : this(elements.SelectMany(item => item.Selected)) {
        }

        /// <summary>
        /// Creates a new CobaltElement using the html provided
        /// </summary>
        public CobaltElement(string html)
            : this(HtmlNode.Parse(html)) {
        }

        /// <summary>
        /// Creates a new CobaltElement using the node provided
        /// </summary>
        public CobaltElement(HtmlNode element)
            : this(new HtmlNode[] { element }) {
        }
        
        /// <summary>
        /// Creates a new CobaltElement using the nodes provided
        /// </summary>
        public CobaltElement(IEnumerable<HtmlNode> elements) {
            this._Selected = elements.ToArray();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the currently selected elements
        /// </summary>
        public virtual IEnumerable<HtmlNode> Selected {
            get { return this._Selected; }
            set { this._Selected = value; }
        }
        private IEnumerable<HtmlNode> _Selected;

        #endregion

        #region Generation

        /// <summary>
        /// Clones and returns a new instance of the CobaltElement
        /// </summary>
        public virtual CobaltElement Clone() {
            return new CobaltElement(this.Selected.Select(node => node.Clone()));
        }

        /// <summary>
        /// Clones the matching elements and returns a new CobaltElement instance
        /// </summary>
        public virtual CobaltElement Clone(string selector) {
            return this.Find(selector).Clone();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Uses the CSS selector to select the correct elements
        /// </summary>
        public virtual CobaltElement Find(string selector) {
            return new CobaltElement(this.FindElements(selector));
        }

        /// <summary>
        /// Uses the CSS selector to select the correct elements
        /// </summary>
        public virtual CobaltElement Find(string selector, Action<CobaltElement> with) {

            //perform the action if needed
            if (with is Action<CobaltElement>) {
                with(new CobaltElement(this.Find(selector)));
            }

            //return the current context
            return this;
        }

        /// <summary>
        /// Adds additional elements to this selection, but doesn't 
        /// move or append them to the document
        /// </summary>
        public virtual CobaltElement Include(ICobaltElement element) {
            return this.Include(new CobaltElement(element));
        }

        /// <summary>
        /// Adds additional elements to this selection, but doesn't 
        /// move or append them to the document
        /// </summary>
        public virtual CobaltElement Include(IEnumerable<ICobaltElement> element) {
            return this.Include(new CobaltElement(element));
        }

        /// <summary>
        /// Adds additional elements to this selection, but doesn't 
        /// move or append them to the document
        /// </summary>
        public virtual CobaltElement Include(IEnumerable<CobaltElement> element) {
            return this.Include(new CobaltElement(element));
        }

        /// <summary>
        /// Adds additional elements to this selection, but doesn't 
        /// move or append them to the document
        /// </summary>
        public virtual CobaltElement Include(CobaltElement element) {
            return this.Include(element.Selected);
        }

        /// <summary>
        /// Adds additional elements to this selection, but doesn't 
        /// move or append them to the document
        /// </summary>
        public virtual CobaltElement Include(HtmlNode element) {
            return this.Include(new HtmlNode[] { element });
        }

        /// <summary>
        /// Adds additional elements to this selection, but doesn't 
        /// move or append them to the document
        /// </summary>
        public virtual CobaltElement Include(IEnumerable<HtmlNode> elements) {
            this.Selected = this.Selected.Union(elements);
            return this;
        }

        /// <summary>
        /// Deselects all nodes for this element
        /// </summary>
        public virtual CobaltElement Deselect() {
            this.Selected = new HtmlNode[] { };
            return this;
        }

        /// <summary>
        /// Deselects all matching nodes for this element
        /// </summary>
        public virtual CobaltElement Filter(HtmlNode node) {
            return this.Filter(new HtmlNode[] { node });
        }

        /// <summary>
        /// Deselects all matching nodes for this element
        /// </summary>
        public virtual CobaltElement Filter(CobaltElement element) {
            return this.Filter(element.Selected);
        }

        /// <summary>
        /// Deselects all matching nodes for this element
        /// </summary>
        public virtual CobaltElement Filter(IEnumerable<HtmlNode> deselect) {
            this.Selected = this.Selected.Except(deselect);
            return this;
        }

        /// <summary>
        /// Removes selected nodes that match the selector
        /// </summary>
        public virtual CobaltElement Filter(string selector) {
            return this.Filter(this.Find(selector));
        }

        #endregion

        #region Working With Elements

        /// <summary>
        /// Applies an action to each of the selected nodes
        /// </summary>
        public virtual CobaltElement Each(Action<CobaltElement> each) {
            IEnumerable<CobaltElement> selected = this.Selected.Select(item => new CobaltElement(item));
            foreach (CobaltElement node in selected) {
                each(node);
            }
            return this;
        }

        #endregion

        #region Styles and Css

        /// <summary>
        /// Adds a class to the selected elements
        /// </summary>
        public virtual CobaltElement AddClass(string @class) {
            return this.Apply(node => node.AddCss(@class));
        }

        /// <summary>
        /// Adds a class to the selected elements
        /// </summary>
        public virtual CobaltElement AddClass(params string[] classes) {
            return this.Apply(node => node.AddCss(classes));
        }

        /// <summary>
        /// Removes a class from the selected elements
        /// </summary>
        public virtual CobaltElement RemoveClass(string @class) {
            return this.Apply(node => node.RemoveCss(@class));
        }

        /// <summary>
        /// Removes classes from the selected elements
        /// </summary>
        public virtual CobaltElement RemoveClass(params string[] classes) {
            return this.Apply(node => node.RemoveCss(classes));
        }

        #endregion

        #region Manipulation

        /// <summary>
        /// Appends items to this a different element
        /// </summary>
        public virtual CobaltElement AppendTo(CobaltElement target) {

            //moves a node into another container
            return this.Apply(child => {
                HtmlNode node = target.Selected.LastOrDefault();
                if (node == null) { return; }
                node.Append(child);
            });

        }

        /// <summary>
        /// Appends items to this Cobalt element
        /// </summary>
        public virtual CobaltElement AppendTo(IEnumerable<HtmlNode> nodes) {
            return this.AppendTo(new CobaltElement(nodes));
        }

        /// <summary>
        /// Appends items to this Cobalt element
        /// </summary>
        public virtual CobaltElement AppendTo(IEnumerable<CobaltElement> elements) {
            return this.AppendTo(new CobaltElement(elements));
        }

        /// <summary>
        /// Appends items to this Cobalt element
        /// </summary>
        public virtual CobaltElement AppendTo(HtmlNode node) {
            return this.AppendTo(new CobaltElement(node));
        }

        /// <summary>
        /// Appends items to this Cobalt element
        /// </summary>
        public virtual CobaltElement AppendTo<T>(T node) where T : ICobaltElement {
            return this.PrependTo(node.AsElement());
        }

        /// <summary>
        /// Appends items to this Cobalt element
        /// </summary>
        public virtual CobaltElement AppendTo<T>(IEnumerable<T> elements) where T : ICobaltElement {
            ICobaltElement target = elements.LastOrDefault();
            return target is ICobaltElement ? this.AppendTo(target) : this;
        }

        /// <summary>
        /// Appends this html to another element
        /// </summary>
        public virtual CobaltElement Append(string html) {
            return this.Append(new CobaltElement(html));
        }

        /// <summary>
        /// Appends the html content to another element
        /// </summary>
        public virtual CobaltElement Append(IEnumerable<string> content) {
            return this.Append(content.Select(html => new CobaltElement(html)));
        }

        /// <summary>
        /// Appends a CobaltElement to another element
        /// </summary>
        public virtual CobaltElement Append(CobaltElement element) {

            //moves a node into another container
            HtmlNode target = this.Selected.LastOrDefault();
            if (target is HtmlNode) {
                target.Append(element.Selected);
            }

            //return this element
            return this;

        }

        /// <summary>
        /// Appends a CobaltElement to another element
        /// </summary>
        public virtual CobaltElement Append(IEnumerable<CobaltElement> elements) {
            return this.Append(new CobaltElement(elements));
        }

        /// <summary>
        /// Appends a CobaltElement to another element
        /// </summary>
        public virtual CobaltElement Append(IEnumerable<HtmlNode> elements) {
            return this.Append(new CobaltElement(elements));
        }

        /// <summary>
        /// Appends an ICobaltElement to another element
        /// </summary>
        public virtual CobaltElement Append<T>(T element) where T : ICobaltElement {
            return this.Append(new CobaltElement(element));
        }

        /// <summary>
        /// Appends a collection of ICobaltElements to another element
        /// </summary>
        public virtual CobaltElement Append<T>(IEnumerable<T> elements) where T : ICobaltElement {
            return this.Append(elements.AsElement());
        }

        /// <summary>
        /// Appends this CobaltElement to another element
        /// </summary>
        public virtual CobaltElement Append(HtmlNode element) {
            return this.Append(new CobaltElement(element));
        }

        /// <summary>
        /// Prepends items to this Cobalt element
        /// </summary>
        public virtual CobaltElement PrependTo(CobaltElement target) {

            //moves a node into another container
            return this.Apply(child => {
                HtmlNode node = target.Selected.LastOrDefault();
                if (node == null) { return; }
                node.Prepend(child);
            });

        }

        /// <summary>
        /// Prepends items to this Cobalt element
        /// </summary>
        public virtual CobaltElement PrependTo(IEnumerable<HtmlNode> nodes) {
            return this.PrependTo(new CobaltElement(nodes));
        }

        /// <summary>
        /// Prepends items to this Cobalt element
        /// </summary>
        public virtual CobaltElement PrependTo(IEnumerable<CobaltElement> elements) {
            return this.PrependTo(new CobaltElement(elements));
        }

        /// <summary>
        /// Prepends items to this Cobalt element
        /// </summary>
        public virtual CobaltElement PrependTo<T>(T node) where T : ICobaltElement {
            return this.PrependTo(node.AsElement());
        }

        /// <summary>
        /// Prepends items to this Cobalt element
        /// </summary>
        public virtual CobaltElement PrependTo<T>(IEnumerable<T> elements) where T : ICobaltElement {
            ICobaltElement target = elements.LastOrDefault();
            return target is ICobaltElement ? this.PrependTo(target) : this;
        }

        /// <summary>
        /// Prepends items to this Cobalt element
        /// </summary>
        public virtual CobaltElement PrependTo(HtmlNode node) {
            return this.PrependTo(new CobaltElement(node));
        }

        /// <summary>
        /// Prepends this html to another element
        /// </summary>
        public virtual CobaltElement Prepend(string html) {
            return this.Prepend(new CobaltElement(html));
        }
        /// <summary>
        /// Prepends the html content to another element
        /// </summary>
        public virtual CobaltElement Prepend(IEnumerable<string> content) {
            return this.Prepend(content.Select(html => new CobaltElement(html)));
        }

        /// <summary>
        /// Prepends this CobaltElement to another element
        /// </summary>
        public virtual CobaltElement Prepend(CobaltElement element) {

            //moves a node into another container
            HtmlNode target = this.Selected.LastOrDefault();
            if (target is HtmlNode) {
                target.Prepend(element.Selected);
            }

            //return this element
            return this;

        }

        /// <summary>
        /// Prepends this CobaltElement to another element
        /// </summary>
        public virtual CobaltElement Prepend(IEnumerable<CobaltElement> elements) {
            return this.Prepend(new CobaltElement(elements));
        }

        /// <summary>
        /// Prepends a CobaltElement to another element
        /// </summary>
        public virtual CobaltElement Prepend(IEnumerable<HtmlNode> elements) {
            return this.Prepend(new CobaltElement(elements));
        }

        /// <summary>
        /// Prepends a CobaltElement to another element
        /// </summary>
        public virtual CobaltElement Prepend(HtmlNode element) {
            return this.Prepend(new CobaltElement(element));
        }

        /// <summary>
        /// Prepends an ICobaltElement to another element
        /// </summary>
        public virtual CobaltElement Prepend<T>(T element) where T : ICobaltElement {
            return this.Prepend(new CobaltElement(element));
        }

        /// <summary>
        /// Prepends a collection of ICobaltElements to another element
        /// </summary>
        public virtual CobaltElement Prepend<T>(IEnumerable<T> elements) where T : ICobaltElement {
            return this.Prepend(elements.AsElement());
        }

        #endregion

        #region Positional Insertion

        /* Not sure these are should be included since they could be
         * inserted into the void... 
         * After(IEnumerable<HtmlNode> elements)
         * After(HtmlNode elements)
         * Before(IEnumerable<HtmlNode> elements)
         * Before(HtmlNode elements)
         */

        /// <summary>
        /// Inserts the current CobaltElement after the target
        /// </summary>
        public virtual CobaltElement InsertAfter(ICobaltElement target) {
            return this.InsertAfter(new CobaltElement(target));
        }

        /// <summary>
        /// Inserts the current CobaltElement after the last target
        /// </summary>
        public virtual CobaltElement InsertAfter<T>(IEnumerable<T> targets) where T : ICobaltElement {
            return this.InsertAfter(targets.AsElement());
        }

        /// <summary>
        /// Inserts the current CobaltElement after the last target
        /// </summary>
        public virtual CobaltElement InsertAfter(IEnumerable<CobaltElement> targets) {
            return this.InsertAfter(new CobaltElement(targets));
        }

        /// <summary>
        /// Inserts the current CobaltElement after the target
        /// </summary>
        public virtual CobaltElement InsertAfter(HtmlNode target) {
            return this.InsertAfter(new CobaltElement(target));
        }

        /// <summary>
        /// Inserts the current CobaltElement after the last target
        /// </summary>
        public virtual CobaltElement InsertAfter(IEnumerable<HtmlNode> targets) {
            return this.InsertAfter(new CobaltElement(targets));
        }

        /// <summary>
        /// Inserts the current CobaltElement after the target
        /// </summary>
        public virtual CobaltElement InsertAfter(CobaltElement target) {
            
            //get the last target since it is the actual one to attach to
            HtmlNode element = target.Selected.HtmlNodes().LastOrDefault() 
                ?? HtmlNode.Parse(string.Empty).FirstOrDefault();

            //move the elements
            return this.Apply(node =>  node.InsertAfter(element));
        }

        /// <summary>
        /// Inserts the current CobaltElement after the last target
        /// </summary>
        public virtual CobaltElement After(ICobaltElement element) {
            return this.After(new CobaltElement(element));
        }

        /// <summary>
        /// Inserts the current CobaltElement after the last target
        /// </summary>
        public virtual CobaltElement After<T>(IEnumerable<T> elements) where T : ICobaltElement {
            return this.After(elements.AsElement());
        }

        /// <summary>
        /// Inserts the provided element after the current CobaltElement
        /// </summary>
        public virtual CobaltElement After(IEnumerable<CobaltElement> elements) {
            return this.After(new CobaltElement(elements));
        }

        /// <summary>
        /// Inserts the provided element after the current CobaltElement
        /// </summary>
        public virtual CobaltElement After(CobaltElement element) {

            //get the last target since it is the actual one to attach to
            HtmlNode target = this.Selected.HtmlNodes().LastOrDefault()
                ?? HtmlNode.Parse(string.Empty).FirstOrDefault();

            //move the elements
            element.Apply(node => node.InsertAfter(target));
            return this;
        }

        /// <summary>
        /// Inserts the current CobaltElement before the target
        /// </summary>
        public virtual CobaltElement InsertBefore(ICobaltElement target) {
            return this.InsertBefore(new CobaltElement(target));
        }

        /// <summary>
        /// Inserts the current CobaltElement before the last target
        /// </summary>
        public virtual CobaltElement InsertBefore<T>(IEnumerable<T> targets) where T : ICobaltElement {
            return this.InsertBefore(targets.AsElement());
        }

        /// <summary>
        /// Inserts the current CobaltElement before the target
        /// </summary>
        public virtual CobaltElement InsertBefore(HtmlNode target) {
            return this.InsertBefore(new CobaltElement(target));
        }

        /// <summary>
        /// Inserts the current CobaltElement before the last target
        /// </summary>
        public virtual CobaltElement InsertBefore(IEnumerable<HtmlNode> targets) {
            return this.InsertBefore(new CobaltElement(targets));
        }

        /// <summary>
        /// Inserts the current CobaltElement before the last target
        /// </summary>
        public virtual CobaltElement InsertBefore(IEnumerable<CobaltElement> targets) {
            return this.InsertBefore(new CobaltElement(targets));
        }

        /// <summary>
        /// Inserts the current CobaltElement before the target
        /// </summary>
        public virtual CobaltElement InsertBefore(CobaltElement target) {

            //get the last target since it is the actual one to attach to
            HtmlNode element = target.Selected.HtmlNodes().LastOrDefault()
                ?? HtmlNode.Parse(string.Empty).FirstOrDefault();

            //move the elements
            return this.Apply(node => node.InsertBefore(element));
        }

        /// <summary>
        /// Inserts the current CobaltElement before the last target
        /// </summary>
        public virtual CobaltElement Before(ICobaltElement element) {
            return this.Before(new CobaltElement(element));
        }

        /// <summary>
        /// Inserts the current CobaltElement before the last target
        /// </summary>
        public virtual CobaltElement Before<T>(IEnumerable<T> elements) where T : ICobaltElement {
            return this.Before(elements.AsElement());
        }

        /// <summary>
        /// Inserts the provided element before the current CobaltElement
        /// </summary>
        public virtual CobaltElement Before(IEnumerable<CobaltElement> elements) {
            return this.Before(new CobaltElement(elements));
        }

        /// <summary>
        /// Inserts the provided element before the current CobaltElement
        /// </summary>
        public virtual CobaltElement Before(CobaltElement element) {

            //get the last target since it is the actual one to attach to
            HtmlNode target = this.Selected.HtmlNodes().LastOrDefault()
                ?? HtmlNode.Parse(string.Empty).FirstOrDefault();

            //move the elements
            element.Apply(node => node.InsertBefore(target));
            return this;
        }

        #endregion

        #region Changing Elements

        /// <summary>
        /// Removes all of the selected items
        /// </summary>
        public virtual CobaltElement Remove() {
            return this.Apply(node => node.Remove());
        }

        /// <summary>
        /// Removes all of the selected items
        /// </summary>
        public virtual CobaltElement Remove(string selector) {
            return this.Apply(selector, node => node.Remove());
        }

        ///// <summary>
        ///// Clears all attributes and children from the selected nodes
        ///// </summary>
        public virtual CobaltElement ClearAll() {
            return this.Empty().ClearAttributes();
        }

        /// <summary>
        /// Removes all content from within the selected elements
        /// </summary>
        public virtual CobaltElement Empty() {
            return this.Apply(node => node.Children.Each(child => child.Remove()));
        }

        /// <summary>
        /// Wraps each of the selected elements with the provided CobaltElement
        /// </summary>
        public virtual CobaltElement Wrap(CobaltElement element) {
            return new CobaltElement(this.Selected.Select(node => element.Clone().InsertBefore(node).Append(node)));
        }

        /// <summary>
        /// Wraps each of the selected elements with the provided CobaltElement
        /// </summary>
        public virtual CobaltElement Wrap(ICobaltElement element) {
            return this.Wrap(new CobaltElement(element));
        }

        /// <summary>
        /// Wraps each of the selected elements with the provided html node
        /// </summary>
        public virtual CobaltElement Wrap(HtmlNode node) {
            return this.Wrap(new CobaltElement(node));
        }

        /// <summary>
        /// Wraps each of the selected elements with the provided html
        /// </summary>
        public virtual CobaltElement Wrap(string html) {
            return this.Wrap(new CobaltElement(html));
        }

        /// <summary>
        /// Wraps the entire CobaltElement with the provided CobaltElement
        /// </summary>
        public virtual CobaltElement WrapAll(CobaltElement element) {
            if (!this.Selected.Any()) { return new CobaltElement(element); }
            return new CobaltElement(element.Clone().InsertBefore(this.Selected.First()).Append(this.Selected));
        }

        /// <summary>
        /// Wraps the entire CobaltElement with the provided CobaltElement
        /// </summary>
        public virtual CobaltElement WrapAll(ICobaltElement element) {
            return this.WrapAll(new CobaltElement(element));
        }

        /// <summary>
        /// Wraps the entire CobaltElement with the provided html node
        /// </summary>
        public virtual CobaltElement WrapAll(HtmlNode node) {
            return this.WrapAll(new CobaltElement(node));
        }

        /// <summary>
        /// Wraps the entire CobaltElement with the provided html
        /// </summary>
        public virtual CobaltElement WrapAll(string html) {
            return this.WrapAll(new CobaltElement(html));
        }

        /// <summary>
        /// Wraps each of the child elements with the provided CobaltElement
        /// </summary>
        public virtual CobaltElement WrapInner(CobaltElement element) {
            return new CobaltElement(this.Children().Wrap(element));
        }

        /// <summary>
        /// Wraps each of the child elements with the provided CobaltElement
        /// </summary>
        public virtual CobaltElement WrapInner(ICobaltElement element) {
            return this.WrapInner(new CobaltElement(element));
        }

        /// <summary>
        /// Wraps each of the child elements with the provided html node
        /// </summary>
        public virtual CobaltElement WrapInner(HtmlNode node) {
            return this.WrapInner(new CobaltElement(node));
        }

        /// <summary>
        /// Wraps each of the child elements with the provided html
        /// </summary>
        public virtual CobaltElement WrapInner(string html) {
            return this.WrapInner(new CobaltElement(html));
        }

        /// <summary>
        /// Detatches the selected elements from the document (but does not
        /// remove them completely)
        /// </summary>
        public virtual CobaltElement Detach() {
            return new CobaltElement(this.Selected.Each(node => node.Detatch()));
        }

        #endregion

        #region Text and Html Content

        /// <summary>
        /// Sets the text value for all selected elements
        /// </summary>
        public virtual CobaltElement Text(string value) {
            return this.Apply(node => node.InnerHtml = HttpUtility.HtmlEncode(value ?? string.Empty));
        }

        /// <summary>
        /// Sets the text value for all selected elements
        /// </summary>
        public virtual CobaltElement Text(object value) {
            return this.Text((value ?? string.Empty).ToString());
        }

        /// <summary>
        /// Sets the text value for all selected elements
        /// </summary>
        public virtual CobaltElement Text(string value, params object[] args) {
            return this.Text(string.Format(value ?? string.Empty, args));
        }

        /// <summary>
        /// Gets the text value for all selected elements
        /// </summary>
        public virtual string Text() {
            return string.Join(string.Empty, this.Selected.Select(node => node.InnerText).ToArray());
        }

        /// <summary>
        /// Sets the html value for all selected elements
        /// </summary>
        public virtual CobaltElement Html(object value) {
            return this.Html((value ?? string.Empty).ToString());
        }

        /// <summary>
        /// Sets the html value for all selected elements
        /// </summary>
        public virtual CobaltElement Html(string value) {
            return this.Apply(node => node.InnerHtml = value);
        }

        /// <summary>
        /// Sets the html value for all selected elements
        /// </summary>
        public virtual CobaltElement Html(string value, params object[] args) {
            return this.Value(string.Format(value ?? string.Empty, args));
        }

        /// <summary>
        /// Gets the text value for all selected elements
        /// </summary>
        public virtual string Html() {
            return string.Join(string.Empty, this.Selected.Select(node => node.InnerHtml).ToArray());
        }

        /// <summary>
        /// Sets the value for all selected elements
        /// </summary>
        public virtual CobaltElement Value(object value) {
            return this.Value((value ?? string.Empty).ToString());
        }

        /// <summary>
        /// Sets the value for all selected elements
        /// </summary>
        public virtual CobaltElement Value(string value, params object[] args) {
            return this.Value(string.Format(value ?? string.Empty, args));
        }

        /// <summary>
        /// Sets the value for all selected elements
        /// </summary>
        public virtual CobaltElement Value(string value) {
            return this.Apply(node => node.SetValue(value));
        }

        /// <summary>
        /// Gets the value for all selected elements
        /// </summary>
        public virtual string Value() {
            return string.Join(string.Empty, this.Selected.Select(node => node.GetValue()).ToArray());
        }

        #endregion

        #region Attributes and CSS

        /// <summary>
        /// Sets the value of attributes for the selected elements
        /// </summary>
        public virtual CobaltElement Attr(object attributes) {
            CobaltAttributePairs created = CobaltAttributePairs.CreateSetFromObject(attributes);
            return this.Attr(created);
        }

        /// <summary>
        /// Sets the value of attributes for the selected elements
        /// </summary>
        public virtual CobaltElement Attr(CobaltAttributePairs attributes) {
            return this.Apply(node => {
                foreach (string key in attributes.Keys) {

                    //format the values
                    string name = (key ?? string.Empty).Trim();
                    object value = attributes[name];

                    //update the attributes
                    node[name] = null;
                    if (value == null) { continue; }
                    node[name] = value;
                }
            });
        }

        /// <summary>
        /// Sets the value of attributes for the selected elements
        /// </summary>
        public virtual CobaltElement Attr(string name, object value) {
            return this.Attr(new CobaltAttributePairs {
                { name, value }
            });
        }

        /// <summary>
        /// Sets the value of attributes for the selected elements
        /// </summary>
        public virtual CobaltElement Attr(string name, string format, params object[] value) {
            return this.Attr(name, string.Format(format, value) as object);
        }

        /// <summary>
        /// Sets the value of attributes for the selected elements
        /// </summary>
        public virtual string Attr(string name) {

            //format the values
            name = (name ?? string.Empty).Trim();

            //apply this attribute
            string value = string.Empty;
            this.Apply(node => {
                if (!node.HasAttribute(name)) { return; }
                value = string.Concat(value, node[name]);
            });

            //return the final value to use
            return value;
        }

        /// <summary>
        /// Removes the list of attributes from an element
        /// </summary>
        public virtual CobaltElement RemoveAttr(params string[] attributes) {
            CobaltAttributePairs pairs = new CobaltAttributePairs();
            foreach (string attribute in attributes) {
                pairs.Add((attribute ?? string.Empty).Trim(), null);
            }
            return this.Attr(pairs);
        }

        /// <summary>
        /// Removes all attributes from an element
        /// </summary>
        public virtual CobaltElement ClearAttributes() {
            return this.Apply(node => node.ClearAttributes());
        }

        #endregion

        #region Information

        /// <summary>
        /// Returns if this element has any selected nodes
        /// </summary>
        public virtual bool Any() {
            return this.Selected.Any();
        }

        /// <summary>
        /// Returns if this element has any selected nodes
        /// </summary>
        public virtual bool Any(string selector) {
            return this.Find(selector).Any();
        }

        /// <summary>
        /// Returns the count of selected nodes
        /// </summary>
        public virtual int Count() {
            return this.Selected.Count();
        }

        /// <summary>
        /// Returns the count of selected nodes
        /// </summary>
        public virtual int Count(string selector) {
            return this.Find(selector).Count();
        }

        /// <summary>
        /// Returns if the selected elements contain this selector
        /// </summary>
        public bool Has(string selector) {
            return this.Selected.MatchesSelector(selector).Any();
        }

        /// <summary>
        /// Returns if the selected elements match this selector
        /// </summary>
        public bool Is(string selector) {
            return this.Selected.MatchesSelector(selector).Count().Equals(this.Selected.Count());
        }

        #endregion

        #region Traversing

        /// <summary>
        /// Returns all of the sibling nodes of this element
        /// </summary>
        public virtual CobaltElement Siblings() {
            return this.Siblings(null, null);
        }

        /// <summary>
        /// Perfoms a set of actions on the current sibling nodes 
        /// </summary>
        public virtual CobaltElement Siblings(Action<CobaltElement> with) {
            return this.Siblings(null, with);
        }

        /// <summary>
        /// Returns all of the sibling nodes of this element
        /// </summary>
        public virtual CobaltElement Siblings(string selector) {
            return this.Siblings(selector, null);
        }

        /// <summary>
        /// Perfoms a set of actions on the current sibling nodes 
        /// </summary>
        public virtual CobaltElement Siblings(string selector, Action<CobaltElement> with) {
            return this.FilterAction(selector, this.Selected.SelectMany(node => node.Siblings), with);
        }

        /// <summary>
        /// Returns all of the parent nodes of this element
        /// </summary>
        public virtual CobaltElement Parents() {
            return this.Parents(null, null);
        }

        /// <summary>
        /// Perfoms a set of actions on the current parent nodes 
        /// </summary>
        public virtual CobaltElement Parents(Action<CobaltElement> with) {
            return this.Parents(null, with);
        }

        /// <summary>
        /// Returns all of the parent nodes of this element
        /// </summary>
        public virtual CobaltElement Parents(string selector) {
            return this.Parents(selector, null);
        }

        /// <summary>
        /// Perfoms a set of actions on the current parent nodes 
        /// </summary>
        public virtual CobaltElement Parents(string selector, Action<CobaltElement> with) {
            return this.FilterAction(selector, this.Selected.Select(node => node.Parent), with);
        }

        /// <summary>
        /// Returns all of the child nodes of this element
        /// </summary>
        public virtual CobaltElement Children() {
            return this.Children(null, null);
        }

        /// <summary>
        /// Perfoms a set of actions on the current child nodes 
        /// </summary>
        public virtual CobaltElement Children(Action<CobaltElement> with) {
            return this.Children(null, with);
        }

        /// <summary>
        /// Returns all of the child nodes of this element
        /// </summary>
        public virtual CobaltElement Children(string selector) {
            return this.Children(selector, null);
        }

        /// <summary>
        /// Perfoms a set of actions on the current child nodes 
        /// </summary>
        public virtual CobaltElement Children(string selector, Action<CobaltElement> with) {
            return this.FilterAction(selector, this.Selected.SelectMany(node => node.Children), with);
        }

        #endregion

        #region Selection

        /// <summary>
        /// Returns the first matching element from this element
        /// </summary>
        public virtual CobaltElement First() {
            HtmlNode matches = this.Selected.FirstOrDefault();
            return new CobaltElement(matches ?? HtmlNode.EmptyNode());
        }

        /// <summary>
        /// Returns the first matching element from this element
        /// </summary>
        public virtual CobaltElement First(string selector) {
            HtmlNode matches = this.Selected
                .MatchesSelector(selector)
                .FirstOrDefault();
            return new CobaltElement(matches ?? HtmlNode.EmptyNode());
        }

        /// <summary>
        /// Returns the last matching element from this element
        /// </summary>
        public virtual CobaltElement Last() {
            HtmlNode matches = this.Selected.LastOrDefault();
            return new CobaltElement(matches ?? HtmlNode.EmptyNode());
        }

        /// <summary>
        /// Returns the last matching element from this element
        /// </summary>
        public virtual CobaltElement Last(string selector) {
            HtmlNode matches = this.Selected
                .MatchesSelector(selector)
                .LastOrDefault();
            return new CobaltElement(matches ?? HtmlNode.EmptyNode());
        }


        /// <summary>
        /// Returns the last matching element from this element
        /// </summary>
        public virtual CobaltElement At(int index) {
            HtmlNode matches = this.Selected
                .MatchesSelector(string.Format("nd({0})", index))
                .FirstOrDefault();
            return new CobaltElement(matches ?? HtmlNode.EmptyNode());
        }

        /// <summary>
        /// Returns the last matching element from this element
        /// </summary>
        public virtual CobaltElement At(string selector, int index) {
            HtmlNode matches = this.Selected
                .MatchesSelector(selector)
                .MatchesSelector(string.Format("nd({0})", index))
                .FirstOrDefault();
            return new CobaltElement(matches ?? HtmlNode.EmptyNode());
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Applies an action to each of the selected nodes
        /// </summary>
        protected internal virtual CobaltElement Apply(string selector, Action<HtmlNode> apply) {
            this.Find(selector).Apply(apply);
            return this;
        }

        /// <summary>
        /// Applies an action to each of the selected nodes
        /// </summary>
        protected internal virtual CobaltElement Apply(Action<HtmlNode> apply) {
            foreach (HtmlNode node in this.Selected.HtmlNodes()) {
                apply(node);
            }
            return this;
        }

        /// <summary>
        /// Selects all matching records from the current element
        /// </summary>
        protected virtual CobaltElement FilterSelection(string selector) {
            return this.FilterSelection(selector, this);
        }

        /// <summary>
        /// Selects all matching records from the set of nodes
        /// </summary>
        protected virtual CobaltElement FilterSelection(string selector, CobaltElement element) {
            return this.FilterSelection(selector, element.Selected);
        }

        /// <summary>
        /// Selects all matching records from the set of nodes
        /// </summary>
        protected virtual CobaltElement FilterSelection(string selector, IEnumerable<HtmlNode> element) {
            CssSelector css = new CssSelector(selector, element);
            return new CobaltElement(css.GetMatches());
        }

        /// <summary>
        /// Performs a CSS selector on only actual HTML nodes (not text nodes)
        /// </summary>
        protected virtual IEnumerable<HtmlNode> FindElements(string selector) {
            CssSelector select = new CssSelector(selector, this.Selected.HtmlNodes());
            return select.GetMatches();
        }

        /// <summary>
        /// Performs a CSS selector on only anything in the selection
        /// </summary>
        protected virtual IEnumerable<HtmlNode> FindAny(string selector) {
            CssSelector select = new CssSelector(selector, this.Selected);
            return select.GetMatches();
        }

        /// <summary>
        /// Common selector used in Cobalt - Allows a set of matches to
        /// be evaluated by an action immediate and then returns the 
        /// current CobaltElement instance to the caller. If no action
        /// is used then the selection is returned instead
        /// </summary>
        protected virtual CobaltElement FilterAction(string selector, IEnumerable<HtmlNode> nodes, Action<CobaltElement> with) {

            //get the nodes and filter as required
            if (selector is string) {
                CssSelector css = new CssSelector(selector, nodes);
                nodes = css.GetMatches();
            }

            //select the new element
            CobaltElement selection = new CobaltElement(nodes);

            //perform the action if needed
            this._WithAction(selection, with);

            //handles returning the correct action - When using
            //a with method, the CobaltElement is considered 
            //disposed so we stick with the current instance
            return with is Action<CobaltElement>
                ? this
                : selection;
        }

        //performs an action (if anything was found)
        private void _WithAction(CobaltElement element, Action<CobaltElement> action) {
            if (action == null) { return; }
            foreach (CobaltElement item in element.Selected.Select(item => new CobaltElement(item))) {
                action(item);
            }
        }

        //performs an action (if anything was found)
        private void _WithAction(CobaltElement element, Action<HtmlNode> action) {
            if (action == null) { return; }
            foreach (HtmlNode item in element.Selected) {
                action(item);
            }
        }

        /// <summary>
        /// Selects HTML nodes without calling the Construct method
        /// which allows templates to load correctly
        /// </summary>
        internal void SelectWithoutConstruct(HtmlNode node) {
            this.SelectWithoutConstruct(new CobaltElement(node));
        }

        /// <summary>
        /// Selects HTML nodes without calling the Construct method
        /// which allows templates to load correctly
        /// </summary>
        internal void SelectWithoutConstruct(ICobaltElement node) {
            this.SelectWithoutConstruct(new CobaltElement(node));
        }

        /// <summary>
        /// Selects HTML nodes without calling the Construct method
        /// which allows templates to load correctly
        /// </summary>
        internal void SelectWithoutConstruct<T>(IEnumerable<T> nodes) where T : ICobaltElement {
            this.SelectWithoutConstruct(nodes.AsElement());
        }

        /// <summary>
        /// Selects HTML nodes without calling the Construct method
        /// which allows templates to load correctly
        /// </summary>
        internal void SelectWithoutConstruct(IEnumerable<CobaltElement> node) {
            this.SelectWithoutConstruct(new CobaltElement(node));
        }

        /// <summary>
        /// Selects HTML nodes without calling the Construct method
        /// which allows templates to load correctly
        /// </summary>
        internal void SelectWithoutConstruct(CobaltElement element) {
            this.SelectWithoutConstruct(element.Selected);
        }

        /// <summary>
        /// Selects HTML nodes without calling the Construct method
        /// which allows templates to load correctly
        /// </summary>
        internal void SelectWithoutConstruct(IEnumerable<HtmlNode> nodes) {
            this._Selected = nodes;
        }

        #endregion

        #region Overriding Methods

        /// <summary>
        /// Returns the markup for this element
        /// </summary>
        public override string ToString() {
            string markup = string.Join(string.Empty, this.Selected.Select(node => node._Container.OuterHtml).ToArray());
            return CobaltContext.Current.RevertHtmlAgilityTextHacks(markup);
        }

        #endregion

        #region Static Creation

        /// <summary>
        /// Creates a CobaltElement from a collection of ICobaltElements
        /// </summary>
        public static CobaltElement FromICobaltElement<T>(T element) where T : ICobaltElement {
            return element.AsElement();
        }

        /// <summary>
        /// Creates a CobaltElement from a collection of ICobaltElements
        /// </summary>
        public static CobaltElement FromICobaltElement<T>(IEnumerable<T> elements) where T : ICobaltElement {
            return elements.AsElement();
        }

        #endregion

        #region ICobaltElement Members

        /// <summary>
        /// Returns the instance of the element
        /// </summary>
        public CobaltElement AsElement() {
            return this;
        }

        #endregion

    }

}

