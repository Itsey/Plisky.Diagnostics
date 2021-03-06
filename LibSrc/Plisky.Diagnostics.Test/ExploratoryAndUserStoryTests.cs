﻿using System.Collections.Generic;
using Plisky.Diagnostics.Copy;
using Plisky.Diagnostics.Listeners;
using System.Threading;
using System.Text.Json;
using Xunit;

namespace Plisky.Diagnostics.Test {



    public class ExploratoryAndUserStoryTests {



        [Fact(DisplayName = nameof(TestProcessStart))]
        [Trait(Traits.Style, Traits.Exploratory)]
        public void TestProcessStart() {
            TCPHandler h = new TCPHandler("127.0.0.1", 9060);
            Bilge sut = TestHelper.GetBilgeAndClearDown();
            sut.AddHandler(h);

            sut.Util.Online("Tester");
            sut.Info.Log("Hello");

            for (int i = 0; i < 10; i++) {
                Thread.Sleep(100);
            }
            sut.Flush();
        }



        [Fact]
        [Trait(Traits.Style, Traits.Exploratory)]
        public void WriteToMex() {
            TCPHandler h = new TCPHandler("127.0.0.1", 9060);
            Bilge sut = TestHelper.GetBilgeAndClearDown();
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
        [Trait(Traits.Style, Traits.Exploratory)]
        public void DiagnosticStringIsValid() {
            Bilge sut = TestHelper.GetBilgeAndClearDown();
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
