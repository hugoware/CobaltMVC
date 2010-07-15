using System.IO;
using System.Web.UI;
using System.Web.UI.Adapters;
using System.Web.Mvc;
using System.Diagnostics;
using System;
using System.Web;

namespace Cobalt.Web.Adapters {

    /// <summary>
    /// Handles rendering Page content correctly
    /// </summary>
    public class CobaltPageAdapter : PageAdapter {

        #region Handling Events

        //set this page as the render container
        protected override void OnInit(EventArgs e) {
            CobaltContext.Current.Page = this.Page;
        }

        //determines the correct way to render this control
        protected override void Render(HtmlTextWriter writer) {

            //if not processing the content
            if (!CobaltContext.Current.Process) {
                base.Render(writer);
                return;
            }

            //if this is a render partial call then avoid
            //doing the rendering - Since the type is isn't
            //visible just check the names
            Type type = this.Page.GetType();
            if (type.Namespace.Equals("System.Web.Mvc") &&
                type.Name.Equals("ViewUserControlContainerPage")) {
                base.Render(writer);
                return;
            }

            //check the page type
            if (this.Page is ViewPage) {
                this._RenderViewPage(writer);
            }
            else {
                this._RenderBasicPage(writer);
            }
        }

        //Renders a ViewPage for ASP.NET MVC
        private void _RenderViewPage(HtmlTextWriter writer) {
            ViewPage page = this.Page as ViewPage;

            //create a new rendering context
            using (StringWriter output = new StringWriter()) {
                using (HtmlTextWriter html = new HtmlTextWriter(output)) {

                    //finish rendering
                    TextWriter original = null;
                    if (page.ViewContext is ViewContext) {
                        original = page.ViewContext.Writer;
                        page.ViewContext.Writer = html;
                    }

                    //update the view
                    base.Render(html);

                    //update the html changes
                    string content = output.ToString();
                    content = CobaltContext.Current.ProcessDocument(this.Page, content);

                    //reset the writer
                    if (original is TextWriter) {
                        page.ViewContext.Writer = original;
                        page.ViewContext.Writer.Write(content);
                    }
                    else {
                        writer.Write(content);
                    }

                    //and display the new content
                }
            }

        }

        //Renders a typical ASP.NET WebForm page
        private void _RenderBasicPage(HtmlTextWriter writer) {

            //create a new rendering context
            using (StringWriter output = new StringWriter()) {
                using (HtmlTextWriter html = new HtmlTextWriter(output)) {

                    //render the content
                    base.Render(html);
                    string content = output.ToString();

                    //process the actions
                    content = CobaltContext.Current.ProcessDocument(this.Page, content);

                    //write the final content
                    writer.Write(content);

                }
            }

        }

        #endregion

    }

}
