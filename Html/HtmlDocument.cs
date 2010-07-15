using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cobalt.Html {

    /// <summary>
    /// Holds the complete document being worked with
    /// </summary>
    public class HtmlDocument {

        #region Constructors

        /// <summary>
        /// Creates a new HTML document
        /// </summary>
        public HtmlDocument(string html)
            : this(HtmlDocument._Parse(html)) {
        }

        /// <summary>
        /// Temporary constructor for working with HtmlAgilityPack
        /// </summary>
        internal HtmlDocument(HtmlAgilityPack.HtmlDocument document) {
            this._Container = document;

            document.OptionAutoCloseOnEnd = true;
            //document.OptionWriteEmptyNodes = true;
            document.OptionFixNestedTags = false;
            document.OptionOutputOriginalCase = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the root element for the document
        /// </summary>
        public HtmlNode Root {
            get { return new HtmlNode(this._Container.DocumentNode); }
        }

        //temporary container for processing documents
        private HtmlAgilityPack.HtmlDocument _Container;

        #endregion

        #region Methods

        /// <summary>
        /// Returns all of the elements in the current document
        /// </summary>
        public IEnumerable<HtmlNode> Descendants() {
            return this.Root.DescendantsAndSelf().Distinct();
        }

        #endregion

        #region Overriding Methods

        /// <summary>
        /// Returns the current HTML for this document
        /// </summary>
        public string Html {
            get { return this.ToString(); }
        }

        /// <summary>
        /// Returns the content of the HtmlDocument
        /// </summary>
        public override string ToString() {
            using (StringWriter writer = new StringWriter()) {
                this._Container.Save(writer);
                return writer.ToString();
            }
        }

        #endregion

        #region Static Creation

        /// <summary>
        /// Parses HTML and returns a new document
        /// </summary>
        public static HtmlDocument Parse(string html) {
            return new HtmlDocument(HtmlDocument._Parse(html));
        }

        //handles parsing HTML until a new parser can be written
        private static HtmlAgilityPack.HtmlDocument _Parse(string html) {

            //generate the html
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(html);

            //return the new element
            return document;
        }

        #endregion

    }

}
