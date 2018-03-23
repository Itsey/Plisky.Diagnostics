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



        [Fact(DisplayName ="InMemory_Works")]
        [Trait("xunit", "fresh")]
        public void InMemory_HoldsOntoMessages() {
            InMemoryHandler imh = new InMemoryHandler();
            var sut = TestHelper.GetBilge();
            sut.AddHandler(imh);
            sut.Info.Log("This is a message.");
            sut.Flush();
            Thread.Sleep(10);
            Assert.Equal(1,imh.GetMessageCount());
        }


        [Fact(DisplayName ="InMemory_ClearWorks")]
        [Trait("xunit", "fresh")]
        public void InMemory_RetrieveClearsMessages() {
            InMemoryHandler imh = new InMemoryHandler();
            var sut = TestHelper.GetBilge();
            sut.AddHandler(imh);
            sut.Info.Log("This is a message.");
            sut.Flush();
            Thread.Sleep(10);
            _ = imh.GetAllMessages();
            Assert.Equal(0, imh.GetMessageCount());
        }

        [Fact(DisplayName = "InMemory_LimitWorks")]
        [Trait("xunit", "fresh")]
        public void InMemory_LimitMessages_Works() {
            InMemoryHandler imh = new InMemoryHandler();
            imh.MaxQueueDepth = 10;
            var sut = TestHelper.GetBilge();
            sut.AddHandler(imh);
            for (int i = 0; i < 100; i++) {
                sut.Info.Log("This is a message.");
            }
            sut.Flush();
            Thread.Sleep(10);            
            Assert.Equal(10, imh.GetMessageCount());
        }




    }
}
