using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cobalt.Interfaces {

    /// <summary>
    /// Instructs that a Model can be converted to a template
    /// </summary>
    public interface ICobaltElement {

        /// <summary>
        /// Returns a CobaltElement to represent the current model
        /// </summary>
        CobaltElement AsElement();
    
    }

}
