using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cobalt {

    /// <summary>
    /// Tracks what part of rendering Cobalt is currently in
    /// </summary>
    public enum CobaltRenderPhase {

        /// <summary>
        /// Cobalt is waiting for the markup to be prepared by ASP.NET
        /// </summary>
        Waiting,

        /// <summary>
        /// The html is being prepared for Ready events
        /// </summary>
        Generate,

        /// <summary>
        /// Ready actions are being invoked and applied to the page
        /// </summary>
        Apply,

        /// <summary>
        /// The final rendering of content is taking place
        /// </summary>
        Rendering,

        /// <summary>
        /// The document has been rendered and content returned for flushing
        /// </summary>
        Completed

    }

}
