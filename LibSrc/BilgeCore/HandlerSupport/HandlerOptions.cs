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
}
