using Plisky.Diagnostics;
using System.Threading;

namespace ConsoleNetFW {
    internal class VerySlowRepository :ARepositry {
        

        public VerySlowRepository() {
            b = new Bilge("VerySlowRepository",tl:System.Diagnostics.TraceLevel.Verbose);
            repoName = "SlowRepo-";
        }

        protected override void ActualDoSomeWork() {
            Thread.Sleep(500);
        }
    }
}