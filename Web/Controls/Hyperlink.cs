using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cobalt.Web.Controls {

    /// <summary>
    /// Creates a new link for a page
    /// </summary>
    public class Hyperlink : CobaltControl {

        #region Constants

        private const string HTML_HYPERLINK = "<a/>";

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty Hyperlink
        /// </summary>
        public Hyperlink() 
            : base(HTML_HYPERLINK) {
        }

        /// <summary>
        /// Creates a new Hyperlink for the target url
        /// </summary>
        public Hyperlink(string url) 
            : base(HTML_HYPERLINK) {
            this.Href = url;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the target value for this link
        /// </summary>
        public string Target {
            get { return this.Attr("target"); }
            set { this.Attr("target", value); }
        }

        /// <summary>
        /// Gets or sets the URL to save to
        /// </summary>
        public string Href {
            get { return this.Attr("href"); }
            set { this.Attr("href", value); }
        }

        /// <summary>
        /// Gets or sets the text value of this link
        /// </summary>
        public string LinkText {
            get { return this.Text(); }
            set { this.Text(value); }
        }

        /// <summary>
        /// Gets or sets the html content for this link
        /// </summary>
        public string LinkHtml {
            get { return this.Html(); }
            set { this.Html(value); }
        }

        #endregion

    }

}
