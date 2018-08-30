using Plisky.Diagnostics;

namespace ConsoleNetFW {
    internal abstract class ARepositry {
        protected string repoName;
        protected Bilge b;
        protected abstract void ActualDoSomeWork();

        public void DoSomeWork() {
            string nme = repoName + nameof(DoSomeWork);
            b.Verbose.TimeStart(nme);
            try {
                b.Info.Log("Doing some work now");
                ActualDoSomeWork();

            } finally {
                b.Verbose.TimeStop(nme);
            }
        }
    }
}