using System.Web.UI;

namespace Cobalt.Web {

    /// <summary>
    /// Control that expects a HttpContext to process with
    /// </summary>
    public abstract class CobaltUserControl : CobaltControl {

        #region Constructors

        /// <summary>
        /// Creates an empty CobaltUserControl
        /// </summary>
        public CobaltUserControl()
            : base() {
        }

        /// <summary>
        /// Creates a new CobaltUserControl using the html provided
        /// </summary>
        public CobaltUserControl(string html)
            : base(html) {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns an instance to the current page
        /// </summary>
        public Page Page {
            get { return CobaltContext.Current.Page; }
        }

        #endregion

    }
}
