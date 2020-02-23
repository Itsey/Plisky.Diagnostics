using Plisky.Diagnostics;

namespace Plisky.Diagnostics.Test {
    internal class FastRepository : ARepositry {

        public FastRepository() {
            b = new Bilge("FastRepository", tl: System.Diagnostics.TraceLevel.Verbose);
            repoName = "FastRepo-";
        }

        protected override void ActualDoSomeWork() {
            
        }
    }
}