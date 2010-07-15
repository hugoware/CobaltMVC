using Cobalt;
using Cobalt.Interfaces;
using Cobalt.Web.Mvc;

namespace System.Web.Mvc {

    /// <summary>
    /// Extensions for Controllers in MVC
    /// </summary>
    public static class ControllerExtensions {

        /// <summary>
        /// Returns a CobaltElement as a View
        /// </summary>
        public static ActionResult Template(this Controller controller, ICobaltElement template) {
            return controller.Element(template.AsElement());
        }

        /// <summary>
        /// Returns a CobaltElement as a View
        /// </summary>
        public static ActionResult Element(this Controller controller, CobaltElement element) {
            return new CobaltResult(element);
        }
    
    }

}
