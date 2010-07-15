using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cobalt {

    /// <summary>
    /// General exception message - Should be replaced later
    /// with more meaningful messages
    /// </summary>
    public class GenericCobaltException : Exception {

        /// <summary>
        /// Throws a generic exception
        /// </summary>
        public GenericCobaltException(string message)
            : base(message) {
        }

        /// <summary>
        /// Throws a generic exception
        /// </summary>
        public GenericCobaltException(string message, Exception inner)
            : base(message, inner) {
        }

    }

}
