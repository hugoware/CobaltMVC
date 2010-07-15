using System;
using System.Linq;
using Cobalt.Interfaces;

namespace Cobalt.Web.Mvc {

    /// <summary>
    /// Identifies the path to a Models template resource
    /// </summary>
    public class ModelTemplateAttribute : Attribute {

        #region Constructors

        /// <summary>
        /// Creates a new ModelTemplate using the page (virtual or physical)
        /// </summary>
        public ModelTemplateAttribute(string path) {
            this.Path = path;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the path for the Model template
        /// </summary>
        public string Path { get; private set; }

        #endregion

        #region Static Methods

        /// <summary>
        /// Attempts to find the ModelTemplateAttribute for a ModelTemplate
        /// </summary>
        public static ModelTemplateAttribute GetAttribute(ICobaltElement template) {

            //model templates should have a path attribute
            ModelTemplateAttribute attribute = template.GetType()
                .GetCustomAttributes(typeof(ModelTemplateAttribute), true)
                .FirstOrDefault() as ModelTemplateAttribute;

            //if this is missing, notify the caller
            if (attribute == null) {
                throw new GenericCobaltException("Models that use IModelTemplate must also declare a ModelTemplateAttribute for the class.");
            }

            //otherwise, load the content
            return attribute;

        }

        #endregion

    }

}
