using Plisky.Diagnostics.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

    }
}
