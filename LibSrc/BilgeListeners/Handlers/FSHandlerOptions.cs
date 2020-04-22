using System;
using System.Collections.Generic;
using System.Text;

namespace Plisky.Diagnostics.Listeners {
    
    public class HandlerOptions {
        public string InitialisationString { get; set; }

        public HandlerOptions(string init) {
            InitialisationString = init;
        }
    }


    public class FSHandlerOptions : HandlerOptions {

        public string FileName  { 
            get {
                return InitialisationString;
            }
        }

        public FSHandlerOptions(string v) : base(v) {
        
        }
    }
}
