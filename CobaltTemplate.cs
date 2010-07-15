using System.IO;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using Cobalt.Html;
using Cobalt.Web;
using Cobalt.Web.Mvc;
using System.Collections.Generic;

namespace Cobalt {

    /// <summary>
    /// CobaltElement designed for loading external templates
    /// </summary>
    public class CobaltTemplate : CobaltControl {

        #region Constructors

        /// <summary>
        /// Loads an external template
        /// </summary>
        public CobaltTemplate(string path) {
            this._Path = path;
            this._LoadTemplate();
        }

        #endregion

        #region Properties

        private string _Path;

        #endregion

        #region Loading Templates

        private Dictionary<string, string> _LoadedTemplates {
            get {
                Dictionary<string, string> templates = HttpContext.Current.Items["LoadedTemplates:Content"] as Dictionary<string, string>;
                if (templates == null) {
                    templates = new Dictionary<string, string>();
                    HttpContext.Current.Items.Add("LoadedTemplates:Content", templates);
                }
                return templates;
            }
        }

        //determines the correct way to load the content
        private void _LoadTemplate() {
            
            //determine how to handle this
            this._Path = (this._Path ?? string.Empty).Trim().ToLower();
            if (this._Path.EndsWith(".aspx")) {
                this._LoadPageTemplate();
            }
            else if (this._Path.EndsWith(".ascx")) {
                this._LoadUserControlTemplate();
            }
            else {
                this._LoadContentTemplate();
            }

        }

        //loads an entire page
        private void _LoadPageTemplate() {
            
            //get and instance of the page
            Control control = BuildManager.CreateInstanceFromVirtualPath(this._Path, typeof(Control)) as Control;

            //get a separate rendering context
            StringWriter write = new StringWriter();
            HttpContext context = new HttpContext(HttpContext.Current.Request, new HttpResponse(write));
            context.Handler = control as IHttpHandler;

            //process and get the content created
            context.Handler.ProcessRequest(context);
            string content = write.ToString();

            //select the content for the view
            this.SelectWithoutConstruct(HtmlNode.Parse(content));

        }

        //loads an external user control
        private void _LoadUserControlTemplate() {
            
            //get a separate rendering context
            StringWriter write = new StringWriter();
            HttpContext context = new HttpContext(HttpContext.Current.Request, new HttpResponse(write));

            //load the control
            CobaltTemplateRenderPage page = new CobaltTemplateRenderPage();
            Control control = page.LoadControl(this._Path);
            control.Page = page;

            //try and render this content
            using (HtmlTextWriter writer = new HtmlTextWriter(write)) {
                control.RenderControl(writer);
            }

            //render the content (which will end up in a context
            //container and should use children instead)
            string content = write.ToString();
            CobaltElement container = new CobaltElement(content);

            //select the content for the view
            this.SelectWithoutConstruct(container.Children());
            
        }

        //loads generic HTML content
        private void _LoadContentTemplate() {

            //update the path if needed
            string path = this._Path.StartsWith("~")
                ? HttpContext.Current.Server.MapPath(this._Path)
                : this._Path;

            //check if this was already loaded
            path = path.ToLower();

            //check in memory for this template
            string content = null;// File.ReadAllText(path);
            if (this._LoadedTemplates.ContainsKey(path)) {
                content = this._LoadedTemplates[path];
            }
            else {
                content = File.ReadAllText(path);
                this._LoadedTemplates.Add(path, content);
            }

            //select the content to use
            this.SelectWithoutConstruct(HtmlNode.Parse(content));
            
        }



        #endregion

    }

}
