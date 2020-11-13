using System;
using System.Collections.Generic;
using System.Text;

namespace Plisky.Diagnostics.Listeners {
    
 


    public class FSHandlerOptions : HandlerOptions {

        public string FileName  { 
            get {
                return InitialisationString;
            }
        }

        public FSHandlerOptions(string v) : base(v) {
        
        }
    }


    public class InMemoryHandlerOptions : HandlerOptions {
        public int MaxQueueDepth { get; set; }
        public bool ClearOnGet { get;  set; }

        public InMemoryHandlerOptions(int queueDepth=5000) : base("") {
            MaxQueueDepth = queueDepth;
            ClearOnGet = true;
        }
    }
}
