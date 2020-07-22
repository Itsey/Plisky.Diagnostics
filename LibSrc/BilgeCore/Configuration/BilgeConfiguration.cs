using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Plisky.Diagnostics {

    public class BilgeConfiguration {
        private List<string> handlers = new List<string>();

        /// <summary>
        /// Indicates whether the existing handlers should all be removed, before attempting to add any of the handlers that are listed in the
        /// HandlerStrings section of the configuration.
        /// </summary>
        public bool StripHandlers { get; set; }


        /// <summary>
        /// Indicates that a new resolver should be created, using the resolver matches to match on the name and that it should set the source
        /// level for each of the matches.  If * is used as a resolver match then ALL requests will match.
        /// </summary>
        public SourceLevels OverallSourceLevel { get; set; }

        /// <summary>
        /// A series of strings that are used to add handlers to the handlers collection. Each one MUST be a properly formatted string, use AddHandler
        /// to add to this array.
        /// </summary>
        public string[] HandlerStrings { 
            get {
                return handlers.ToArray();
            }
        }

        /// <summary>
        /// A list of string matches for a global resolver, used to filter how new Bilge instances are created. Use * to match all instance creates.
        /// </summary>
        public string[] ResolverMatches { get; set; }

        public void ApplyConfiguration() {
            //[TCPD:127.0.0.1:9060]
            //[FILE:c:\temp\test\test]
            //[OUDS:]
            //[HTTP:http://test/test/test]
        }

        public BilgeConfiguration() {
           
            ResolverMatches = new string[] { };
        }

        /// <summary>
        /// Adds a well formed string to the array of handlers that are to be added.
        /// </summary>
        /// <param name="v"></param>
        public void AddHandlerString(string v) {
            
        }
    }
}
