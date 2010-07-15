using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Web.Mvc.Html;
using System.IO;

namespace Cobalt.Web.Mvc {

    /// <summary>
    /// Returns a CobaltElement as an ActionResult
    /// </summary>
    public class CobaltResult : ActionResult {

        #region Constructors

        /// <summary>
        /// Returns a CobaltElement as an ActionResult
        /// </summary>
        public CobaltResult(CobaltElement element)
            : this(element, "text/html") {
        }

        /// <summary>
        /// Returns a CobaltElement as an ActionResult
        /// </summary>
        public CobaltResult(CobaltElement element, string contentType)
            : this(element, contentType, Encoding.UTF8) {
        }

        /// <summary>
        /// Returns a CobaltElement as an ActionResult
        /// </summary>
        public CobaltResult(CobaltElement element, string contentType, Encoding encoding) {
            this.Element = element;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The element to render to the client
        /// </summary>
        public CobaltElement Element { get; private set; }

        /// <summary>
        /// Holds the type of content to render this as
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// The content this result should be rendered as 
        /// </summary>
        public Encoding Encoding { get; private set; }

        #endregion

        #region Required Methods

        /// <summary>
        /// Renders the correct result to the client
        /// </summary>
        public override void ExecuteResult(ControllerContext context) {

            //finalize the content
            if (this.Element is CobaltControl) {
                (this.Element as CobaltControl).PerformFinalize();
            }

            //execute a content result with the markup
            ContentResult result = new ContentResult() {
                Content = this.Element.ToString(),
                ContentType = this.ContentType,
                ContentEncoding = this.Encoding
            };

            //and finalize the content
            result.ExecuteResult(context);
        }

        #endregion

    }
}
