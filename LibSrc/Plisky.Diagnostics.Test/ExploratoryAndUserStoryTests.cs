using Plisky.Diagnostics.Listeners;
using System.Threading;
using Xunit;

namespace Plisky.Diagnostics.Test {
    public class ExploratoryAndUserStoryTests {

    

        [Fact]
        [Trait("XUnit", "usecase")]
        public void WriteToMex() {
            TCPHandler h = new TCPHandler("127.0.0.1", 9060);
            Bilge sut = TestHelper.GetBilge();
            sut.AddHandler(h);

            sut.Info.Log("Hellow cruiel world");
            sut.Info.Log("Hellow cruiel world");
            sut.Info.Log("Hellow cruiel world");
            sut.Info.Log("Hellow cruiel world");
            sut.Info.Log("Hellow cruiel world");
            sut.Info.Log("Hellow cruiel world");
            for (int i = 0; i < 10; i++) {
                Thread.Sleep(300);
            }
        }


        [Fact]
        public void DiagnosticStringIsValid() {
            Bilge sut = TestHelper.GetBilge();
            sut.AddHandler(new TCPHandler("192.168.1.7", 9060));
            sut.Info.Log("Hi");
            sut.Info.Log("Hi");
            sut.Info.Log("Hi");
            sut.Flush();



            for (int i = 0; i < 10; i++) {
                Thread.Sleep(300);
            }
            string s = sut.GetDiagnosticStatus();
            Assert.NotNull(s);
        }



    }
}
