using System.Collections.Generic;
using System.Web.UI;
using Cobalt.Html;
using Cobalt.Web;

namespace Cobalt {

    /// <summary>
    /// CobaltElement that behaves as a UserControl
    /// </summary>
    public abstract class CobaltControl : CobaltElement {

        #region Constructors

        /// <summary>
        /// CobaltControls may create empty elements
        /// </summary>
        protected CobaltControl() :
            base(new HtmlNode[] { }) {
            this._PrepareCobaltControl();
        }

        /// <summary>
        /// Creates a new CobaltControl from the provided Html
        /// </summary>
        public CobaltControl(string html)
            : base(HtmlNode.Parse(html)) {
            this._PrepareCobaltControl();
        }

        #endregion

        #region Properties

        //has this control been built yet or not
        private bool _HasBeenConstructed;

        //holds if the control has performed the finalize phase
        private bool _HasFinalized;

        /// <summary>
        /// Returns the currently selected nodes for this control
        /// </summary>
        public override IEnumerable<HtmlNode> Selected {
            get {
                this._CheckForConstruction();
                return base.Selected;
            }
            set {
                this._CheckForConstruction();
                base.Selected = value;
            }
        }

        #endregion

        #region Event Methods

        /// <summary>
        /// Called before this control can be queried to allow
        /// any setup code to run in advance
        /// </summary>
        protected virtual void OnConstruct() { }

        /// <summary>
        /// Used to finalize any changes after Ready actions have been applied
        /// </summary>
        protected virtual void OnFinalize() { }

        #endregion

        #region Private Methods

        //prepares the construct event to be called
        private void _PrepareCobaltControl() {
            this._HasBeenConstructed = false;
            this._HasFinalized = false;
            CobaltContext.Current.RegisterFinalizeEvent(CobaltContext.Current.Rendering, this.PerformFinalize);
        }

        //performs the contruct event if needed
        private void _CheckForConstruction() {
            if (this._HasBeenConstructed) { return; }
            this._HasBeenConstructed = true;
            this.OnConstruct();
        }

        /// <summary>
        /// Forces a call to OnFinalize
        /// </summary>
        internal void PerformFinalize() {
            if (this._HasFinalized) { return; }
            this._HasFinalized = true;
            this.OnFinalize();
        }

        #endregion

    }

}
