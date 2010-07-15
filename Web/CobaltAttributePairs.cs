using System.Collections.Generic;
using System.Reflection;

namespace Cobalt.Web {

    /// <summary>
    /// A collection of attributes to append to a CobaltElement
    /// </summary>
    public class CobaltAttributePairs : Dictionary<string, object> {

        #region Static Methods

        /// <summary>
        /// Reads an anonymous type to determine properties and values
        /// </summary>
        public static CobaltAttributePairs CreateSetFromObject(object value) {
            CobaltAttributePairs created = new CobaltAttributePairs();
            foreach (PropertyInfo property in value.GetType().GetProperties()) {
                created.Remove(property.Name);
                created.Add(property.Name, property.GetValue(value, null));
            }

            //apply them to the element
            return created;
        }

        #endregion

    }

}
