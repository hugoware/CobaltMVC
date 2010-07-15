using System;
using System.Web.UI;
using System.Web.UI.Adapters;

namespace Cobalt.Web.Adapters {

    /// <summary>
    /// Handles rendering UserControl content into the correct containers
    /// </summary>
    public class CobaltControlAdapter : ControlAdapter {

        #region Handling Events

        //determines the correct way to render this control
        protected override void Render(HtmlTextWriter writer) {

            //determine if a template block should be wrapped
            if (!(this.Page is CobaltTemplateRenderPage) || 
                (CobaltContext.Current == null || 
                !CobaltContext.Current.Process || 
                CobaltContext.Current.Phase != CobaltRenderPhase.Waiting)) {

                //render this block to be found later
                this._RenderControlContentWithContextBlocks(writer);

            }
            //otherwise, just render normally
            else {
                base.Render(writer);
            }


        }

        //handles rendering blocks to wrap an element and 
        //returns the correct identity
        private string _RenderControlContentWithContextBlocks(HtmlTextWriter writer) {

            //get a unique identity for this content
            string identity = Guid.NewGuid().ToString();

            //write the context information to the page
            writer.Write(string.Format("<{0} id=\"{1}\">", CobaltConfiguration.CONTROL_CONTEXT_ELEMENT, identity));
            base.Render(writer);
            writer.Write(string.Format("</{0}>", CobaltConfiguration.CONTROL_CONTEXT_ELEMENT));

            //register this block to be found later
            CobaltContext.Current.RegisterControlContext(this.Control, identity);

            //return the identity for this block
            return identity;
        }

        #endregion

    }

}
