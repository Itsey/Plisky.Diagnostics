using Plisky.Diagnostics.Listeners;
using System.Threading;
using Xunit;

namespace Plisky.Diagnostics.Test {


    /// <summary>
    /// These are designed to run in conjunction with FlimFlam, there may be the odd assert but this is mostly there for checkign the test
    /// results within flimflam directly not through the testing framework.  Essentially creating a dummy UI using test cases not buttons.
    /// </summary>
    public class FullUseCaseTests {
        private Bilge b;
        public FullUseCaseTests() {
            b = new Bilge(tl:System.Diagnostics.TraceLevel.Verbose);
            b.AddHandler(new TCPHandler("127.0.0.1", 9060));
        }

        [Fact(DisplayName = nameof(TestProcess_Online))]    
        public void TestProcess_Online() {
            b.Util.Online("Tester");
            b.Info.Log("Hello");
            b.Info.Log("WOrld");
            b.Flush();
        }





    }
}
