using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Cobalt.Html;
using Cobalt.Css;

namespace Cobalt {

    /// <summary>
    /// Holds general configuration for Cobalt
    /// </summary>
    public static class CobaltConfiguration {

        #region Constants

        internal const string CONTROL_CONTEXT_ELEMENT = "cobaltelementcontext";

        #endregion

        #region Methods

        /// <summary>
        /// Prepares Cobalt to be used with this site
        /// </summary>
        public static void Initialize() {
            CobaltConfiguration.VerifyBrowserFile();
        }

        /// <summary>
        /// Ensures that the browser file required is created in
        /// the project
        /// </summary>
        public static void VerifyBrowserFile() {

            //get the path
            string browsers = HttpContext.Current.Server.MapPath("~/App_Browsers/");

            //check the directory first
            if (!Directory.Exists(browsers)) {
                Directory.CreateDirectory(browsers);
            }

            //check for the file
            string file = Path.Combine(browsers, "Cobalt.browser");
            if (!File.Exists(file)) {
                File.WriteAllText(file, Resources.Resources.BrowserFile);
            }

        }

        /// <summary>
        /// Registers a custom pseudo selector
        /// </summary>
        public static void RegisterCustomPseudoSelector(string name, Func<IEnumerable<HtmlNode>, string, IEnumerable<HtmlNode>> compare) {
            CssSelectorDetail.RegisterCustomPseudoSelector(name, compare);
        }

        /// <summary>
        /// Removes a custom pseudo selector if any for the name exists
        /// </summary>
        public static void UnregisterCustomPseudoSelector(string name) {
            CssSelectorDetail.UnregisterCustomPseudoSelector(name);
        }

        /// <summary>
        /// Registers a custom attribute selector
        /// </summary>
        public static void RegisterCustomAttributeSelector(string type, Func<string, string, bool> compare) {
            CssSelectorDetail.RegisterCustomAttributeSelector(type, compare);
        }

        /// <summary>
        /// Removes an existing custom attribute selector
        /// </summary>
        public static void UnregisterCustomAttributeSelector(string type) {
            CssSelectorDetail.UnregisterCustomAttributeSelector(type);
        }

        #endregion

    }

}
