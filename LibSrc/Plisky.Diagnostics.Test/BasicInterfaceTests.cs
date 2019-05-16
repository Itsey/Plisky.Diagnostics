namespace Plisky.Diagnostics.Test {

    using Plisky.Diagnostics;
    //using Plisky.Test;
    using System.Threading;
    using Xunit;



    public class BasicInterfaceTests {
        private void WriteASeriesOfMessages(Bilge b) {
            b.Info.Log("Test message");
            b.Verbose.Log("test message");
            b.Error.Log("Test message");
            b.Warning.Log("Test message");
        }


        [Fact(DisplayName = nameof(MessageBatching_Works_Default1))]
       /// [Trait(Traits.Age, Traits.Fresh)]
        //[Trait(Traits.Style, Traits.Unit)]
        public void MessageBatching_Works_Default1() {
            Bilge sut = TestHelper.GetBilge();
            sut.CurrentTraceLevel = System.Diagnostics.TraceLevel.Info;
            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);


            sut.Info.Log("Dummy Message");
            sut.Flush();
            sut.Info.Log("Dummy Message");
            sut.Flush();
            Assert.Equal(1, mmh.LastMessageBatchSize);

        }

        [Fact(DisplayName = nameof(MessageBatching_Works_Enabled))]
        /// [Trait(Traits.Age, Traits.Fresh)]
        //[Trait(Traits.Style, Traits.Unit)]
        public void MessageBatching_Works_Enabled() {
            const int MESSAGE_BATCHSIZE = 10;

            Bilge sut = TestHelper.GetBilge();

            sut.SetMessageBatching(MESSAGE_BATCHSIZE, 0);

            sut.CurrentTraceLevel = System.Diagnostics.TraceLevel.Info;
            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);

            for (int i = 0; i < 100; i++) {
                sut.Info.Log("Dummy Message");

                if (i%25==0) {
                    // The flush forces the write, this is needed otherwise it bombs through
                    // too fast for more than one write to the handler to occur.
                    sut.Flush();
                }

                if (mmh.TotalMessagesRecieved > 0) {
                    // Any time that we get a batch it must be at least MESSAGE_BATCHSIZE msgs.
                    Assert.True(mmh.LastMessageBatchSize >= MESSAGE_BATCHSIZE);
                }
            }
            

        }


        [Fact]
        [Trait("xunit", "regression")]
        public void Enter_WritesMethodName() {
            Bilge sut = TestHelper.GetBilge();
            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);
            sut.Info.E();

            sut.Flush();

            mmh.SetMustContainForBody(nameof(Enter_WritesMethodName));

            // E generates more than one message, therefore we have to check that one of the messages had the name in it.
            mmh.AssertAllConditionsMetForAllMessages(true, true);

        }

        [Fact]
        [Trait("xunit", "regression")]
        public void Exit_WritesMethodName() {
            Bilge sut = TestHelper.GetBilge();
            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);
            sut.Info.X();

            sut.Flush();

            mmh.SetMustContainForBody(nameof(Exit_WritesMethodName));

            // E generates more than one message, therefore we have to check that one of the messages had the name in it.
            mmh.AssertAllConditionsMetForAllMessages(true, true);

        }


        [Fact]
        [Trait("xunit", "regression")]
        public void Bilge_EnterSection_TracesSection() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge();
            sut.AddHandler(mmh);
            mmh.SetMethodNameMustContain("monkeyfish");
            sut.Info.EnterSection("monkeyfish");

            for (int i = 0; i < 10; i++) {
                Thread.Sleep(300);
            }

            mmh.AssertAllConditionsMetForAllMessages(true, true);

        }

        [Fact]
        [Trait("xunit", "regression")]
        public void Bilge_LeaveSection_TracesSection() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge();
            sut.AddHandler(mmh);

            mmh.SetMethodNameMustContain("bannanaball");
            sut.Info.LeaveSection("bannanaball");

            for (int i = 0; i < 10; i++) {
                Thread.Sleep(300);
            }

            mmh.AssertAllConditionsMetForAllMessages(true, true);

        }

        [Fact]
        [Trait("xunit", "regression")]
        public void Assert_True_DoesNothingIfTrue() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge();
            sut.AddHandler(mmh);

            sut.Assert.True(true);
            Assert.Equal(0, mmh.AssertionMessageCount);
        }

        [Fact]
        [Trait("xunit", "regression")]
        public void Assert_True_DoesFailsIfFalse() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge();
            sut.AddHandler(mmh);

            sut.Assert.True(false);

            for (int i = 0; i < 10; i++) {
                Thread.Sleep(300);
            }


            Assert.True(mmh.AssertionMessageCount > 0);
        }

        [Fact]
        [Trait("xunit", "regression")]
        public void Assert_False_FailsIfTrue() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge();
            sut.AddHandler(mmh);
            sut.Assert.True(false);

            for (int i = 0; i < 10; i++) {
                Thread.Sleep(300);
            }


            Assert.True(mmh.AssertionMessageCount > 0);
        }

        [Fact]
        [Trait("xunit", "regression")]
        public void Assert_False_DoesNothingIfFalse() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge();
            sut.AddHandler(mmh);
            sut.Assert.True(true);
            Assert.Equal(0, mmh.AssertionMessageCount);
        }



        [Fact]
        [Trait("xunit", "regression")]
        public void Flow_WritesMethodNameToMessage() {
            MockMessageHandler mmh = new MockMessageHandler();
            mmh.SetMustContainForBody(nameof(Flow_WritesMethodNameToMessage));
            var sut = TestHelper.GetBilge();
            sut.AddHandler(mmh);

            sut.Info.Flow();

            sut.Flush();
            mmh.AssertAllConditionsMetForAllMessages(true);
        }




    }

}