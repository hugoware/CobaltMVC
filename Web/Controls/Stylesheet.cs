using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cobalt.Html;
using Cobalt.Web;

namespace Cobalt.Web.Controls {

    /// <summary>
    /// A link element generated to act as a stylesheet
    /// </summary>
    public class Stylesheet : CobaltUserControl {

        #region Constructors

        /// <summary>
        /// Creates a new stylesheet element
        /// </summary>
        public Stylesheet(string href)
            : base("<link/>") {
            this.Href = href;
            this.Rel = "stylesheet";
            this.Type = "text/css";
            this.RemoveDuplicates = true;
        }

        #endregion

        #region Static Creation

        /// <summary>
        /// Creates and appends a new stylesheet to this page
        /// </summary>
        public static Stylesheet Append(string href) {
            Stylesheet sheet = new Stylesheet(href);
            sheet.AppendToHead();
            return sheet;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Removes matching stylesheets from the head element
        /// </summary>
        public virtual bool RemoveDuplicates { get; set; }

        /// <summary>
        /// Gets or sets the target of this stylesheet
        /// </summary>
        public virtual string Href {
            get { return this.Attr("href"); }
            set { this.Attr("href", value); }
        }

        /// <summary>
        /// Gets or sets the media type for this stylesheet
        /// </summary>
        public virtual string Media {
            get { return this.Attr("media"); }
            set { this.Attr("media", value); }
        }

        /// <summary>
        /// Gets or sets the relationship for this element
        /// </summary>
        public virtual string Rel {
            get { return this.Attr("rel"); }
            set { this.Attr("rel", value); }
        }

        /// <summary>
        /// Gets or sets the type of element this is
        /// </summary>
        public virtual string Type {
            get { return this.Attr("type"); }
            set { this.Attr("type", value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Automatically moves this control to the head element
        /// </summary>
        public virtual void AppendToHead() {
            CobaltContext.Current.Document.Find("head").Append(this);
        }

        #endregion

        #region Events

        //prepares the control to be displayed
        protected override void OnConstruct() { }

        //finalizes any changes
        protected override void OnFinalize() {
            if (!this.RemoveDuplicates) { return; }
            
            //find and remove matching paths
            string path = string.Format("head link[@href='{0}']", this.Href);
            this.Page.Find(path)
                .Selected
                .Skip(1)
                .Each(node => node.Remove());
        }

        #endregion

    }

}
