using Plisky.Diagnostics.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Plisky.Diagnostics.Test {
    public class InMemoryHandlerTests {

        [Fact(DisplayName = "InMemory_Works")]
        [Trait("xunit", "fresh")]
        public void InMemory_HoldsOntoMessages() {
            var imh = new InMemoryHandler();
            var sut = TestHelper.GetBilge();
            sut.AddHandler(imh);
            sut.Info.Log("This is a message.");
            sut.Flush();
            Thread.Sleep(10);
            Assert.Equal(1, imh.GetMessageCount());
        }


        [Fact(DisplayName = "InMemory_ClearWorks")]
        [Trait("xunit", "fresh")]
        public void InMemory_RetrieveClearsMessages() {
            var imh = new InMemoryHandler();
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
            var imh = new InMemoryHandler();
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
