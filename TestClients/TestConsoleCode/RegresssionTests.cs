using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plisky.Diagnostics.Test {

    public class RegresssionTests {
        protected Bilge b;

        public RegresssionTests() {
            
        }

        public void RunTests() {
            ExampleSubsystem.Run(b);
            ExampleSubsystem.Run2(b);
        }

        internal void Prepare(string initString) {
            b = new Bilge(tl: System.Diagnostics.TraceLevel.Verbose);
            b.AddHandler(new TCPHandler("127.0.0.1", 9060));

            b.Verbose.Log(initString);
        }

        internal void AllDone() {
            b.Info.Log("All Done.");
            b.Flush();
        }
    }
}
