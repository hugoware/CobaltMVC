using System;
using System.IO;
using System.Web.UI;

namespace Cobalt {

    /// <summary>
    /// CobaltControl that is tied directly to a WebControl
    /// </summary>
    public class CobaltWebControl<T> : CobaltControl where T : Control {

        #region Constructors

        /// <summary>
        /// Creates a WebControl without arguments
        /// </summary>
        public CobaltWebControl()
            : this(null) {
        }

        /// <summary>
        /// Creates a control using the provided arguments
        /// </summary>
        public CobaltWebControl(params object[] args)
            : base() {
            this._LoadControl(args);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The control that has been created
        /// </summary>
        private T _Control;

        #endregion

        #region Events

        /// <summary>
        /// Called after the control is created but before
        /// it is rendered or modified
        /// </summary>
        protected virtual void OnCreateControl(T control) {
        }

        #endregion

        #region Private Methods

        //loads the html content into memory
        private void _LoadControl(object[] args) {

            //create the control and notify
            this._Control = Activator.CreateInstance(typeof(T), args) as T;
            this.OnCreateControl(this._Control);

            //then render the content
            using (StringWriter writer = new StringWriter()) {
                using (HtmlTextWriter html = new HtmlTextWriter(writer)) {
                    this._Control.RenderControl(html);
                    string content = writer.ToString();
                    this.SelectWithoutConstruct(new CobaltElement(content));
                }
            }
        }

        #endregion

    }

}
