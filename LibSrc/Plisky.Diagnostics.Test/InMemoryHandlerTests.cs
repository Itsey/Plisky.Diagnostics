using Plisky.Diagnostics.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plisky.Diagnostics.Copy;
using Xunit;

namespace Plisky.Diagnostics.Test {
    public class InMemoryHandlerTests {

        [Fact(DisplayName = nameof(BilgeRoutesToInMemory))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Integration)]
        public void BilgeRoutesToInMemory() {
            var imh = new InMemoryHandler();
            var sut = TestHelper.GetBilgeAndClearDown();
            sut.AddHandler(imh);
            sut.Info.Log("This is a message.");
            sut.Flush();
            Assert.Equal(1, imh.GetMessageCount());
        }




        [Fact(DisplayName = nameof(LimitMessageCount_Works))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public async void LimitMessageCount_Works() {
            var imh = new InMemoryHandler();
            imh.MaxQueueDepth = 10;

            var gms = TestHelper.GetMessageMetaData(100);
            await imh.HandleMessageAsync(gms);
            
            Assert.Equal(10, imh.GetMessageCount());
        }



        [Fact(DisplayName = nameof(DefaultFormatter_Pretty))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public async void DefaultFormatter_Pretty() {

            var sut = new InMemoryHandler();
            await sut.HandleMessageAsync(TestHelper.GetMessageMetaData());

            var f = sut.GetAllMessages();
            Assert.Equal(1, f.Length);
        }



        [Fact(DisplayName = nameof(InMemory_DefaultsToClearOnGet))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void InMemory_DefaultsToClearOnGet() {
            var sut = new InMemoryHandler();
            Assert.True(sut.ClearOnGet);

        }


        [Fact(DisplayName = nameof(InMem_WithClearSet_GetClears))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public async void InMem_WithClearSet_GetClears() {

            var sut = new InMemoryHandler();
            await sut.HandleMessageAsync(TestHelper.GetMessageMetaData());
            sut.ClearOnGet = true;
            _ = sut.GetAllMessages().Length;
            var second = sut.GetAllMessages().Length;
            Assert.Equal(0, second);

        }

        [Fact(DisplayName = nameof(InMem_WithClearNotSet_GetDoesNotClear))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public async void InMem_WithClearNotSet_GetDoesNotClear() {

            var sut = new InMemoryHandler();
            await sut.HandleMessageAsync(TestHelper.GetMessageMetaData());
            sut.ClearOnGet = false;
            var first = sut.GetAllMessages().Length;
            var second = sut.GetAllMessages().Length;
            Assert.Equal(first,second);
        }


        [Fact(DisplayName = nameof(GetMessage_ClearSet_Clears))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public async void GetMessage_ClearSet_Clears() {

            var sut = new InMemoryHandler();
            await sut.HandleMessageAsync(TestHelper.GetMessageMetaData());
            sut.ClearOnGet = true;
            _ = sut.GetMessage();
            Assert.Null(sut.GetMessage());

        }

        [Fact(DisplayName = nameof(GetMessage_NoClear_DoesNotClear))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public async void GetMessage_NoClear_DoesNotClear() {

            var sut = new InMemoryHandler();

            await sut.HandleMessageAsync(TestHelper.GetMessageMetaData());
            sut.ClearOnGet = false;
            var first = sut.GetAllMessages().Length;
            var second = sut.GetAllMessages().Length;
            Assert.Equal(first, second);
        }


        [Fact(DisplayName = nameof(InMemoryHandler_UsesUniquenessRef))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public async void InMemoryHandler_UsesUniquenessRef() {

            var sut = new InMemoryHandler();
            sut.SetFormatter(new FlimFlamV2Formatter());
            sut.ClearOnGet = false;

            var msg = TestHelper.GetMessageMetaData();
            await sut.HandleMessageAsync(msg);
            sut.SetFormatter(new FlimFlamV2Formatter());

            string first = sut.GetMessage();
            string second = sut.GetMessage();
            await sut.HandleMessageAsync(msg);
            

            ///When a formatter does not contain a uniqueness reference this string is used in its place
            Assert.DoesNotContain(BaseMessageFormatter.DEFAULT_UQR, first);
            Assert.Equal(first, second);

        }

        [Fact(DisplayName = nameof(InMemoryHandler_UsesUniquenessRef))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public async void EachMessage_HasUniqueness() {

            var sut = new InMemoryHandler();
            sut.SetFormatter(new FlimFlamV2Formatter());
            sut.ClearOnGet = false;

            var msg = TestHelper.GetMessageMetaData(10);
            await sut.HandleMessageAsync(msg);
            sut.SetFormatter(new FlimFlamV2Formatter());

            var allMsgs = sut.GetAllMessages();
            List<string> pastMessages = new List<string>();

            foreach(var f in allMsgs) {
               foreach(var l in pastMessages) {
                    Assert.NotEqual(l, f);
               }
                pastMessages.Add(f);
            }
        }
    }
}
