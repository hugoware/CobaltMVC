using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Web;

namespace Cobalt {

    /// <summary>
    /// Utility class for tracking performance for sections of code
    /// </summary>
    internal static class Log {

        //the HttpContext.Current.Item container
        private const string HTTPITEM_STOPWATCHES = "Stopwatches:Logging";

        /// <summary>
        /// Traces a message 
        /// </summary>
        public static void Write(object context, string message, params object[] parameters) {
            Stopwatch watch = Log._GetStopwatchFor(context);
            Trace.WriteLine(string.Format("{0}ms: {1}", watch.ElapsedMilliseconds, string.Format(message, parameters)));
        }

        //grabs the correct stopwatch to monitor
        private static Stopwatch _GetStopwatchFor(object context) {

            //get the stop watches to use
            Dictionary<object, Stopwatch> timers = HttpContext.Current.Items[HTTPITEM_STOPWATCHES] as Dictionary<object, Stopwatch>;
            if (timers == null) {
                timers = new Dictionary<object, Stopwatch>();
                HttpContext.Current.Items.Add(HTTPITEM_STOPWATCHES, timers);
            }

            //return the correct value
            if (timers.ContainsKey(context)) {
                return timers[context];
            }
            else {
                Stopwatch watch = new Stopwatch();
                timers.Add(context, watch);
                watch.Start();
                return watch;
            }
        }
    
    }

}
