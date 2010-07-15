using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cobalt.Css {

    /// <summary>
    /// Scopes for Css selectors
    /// </summary>
    public enum CssSelectorScope {

        /// <summary>
        /// Matches anything including the starting
        /// nodes - Default for a new CssSelector search
        /// </summary>
        Any,

        /// <summary>
        /// Matches any current or child element
        /// </summary>
        AnyChild,

        /// <summary>
        /// Scopes to only direct children of the current selection
        /// </summary>
        DirectChildren,

        /// <summary>
        /// Finds all siblings of the current selection
        /// </summary>
        Siblings,

        /// <summary>
        /// Limits the selection to only the currently selected nodes
        /// </summary>
        Selected

    }

}
