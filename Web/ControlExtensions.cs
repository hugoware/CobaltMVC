using System;
using System.Web.UI;

namespace Cobalt.Web {

    /// <summary>
    /// Extensions for starting commands Cobalt
    /// </summary>
    public static class ControlExtensions {

        /// <summary>
        /// Registers work to perform after the document becomes ready
        /// </summary>
        public static void Ready(this Control control, Action action) {
            CobaltContext.Current.RegisterReadyAction(control, action);
        }

        /// <summary>
        /// Finds elements for the current context
        /// </summary>
        public static CobaltElement Find(this Control control, string selector) {
            return control.Find(selector, null);
        }

        /// <summary>
        /// Finds elements for the current context and performs an action
        /// on them immediately
        /// </summary>
        public static CobaltElement Find(this Control control, string selector, Action<CobaltElement> with) {
            ReadyContext context = CobaltContext.Current.FindReadyContextByInstance(control);
            CobaltElement element = new CobaltElement(context.Nodes);
            return element.Find(selector, with);
        }

        /// <summary>
        /// Creates an entirely new CobaltElement with the html provided
        /// </summary>
        public static CobaltElement Create(this Control control, string html) {
            return new CobaltElement(html);
        }
    
    }

}
