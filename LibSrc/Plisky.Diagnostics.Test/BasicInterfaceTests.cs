namespace Plisky.Diagnostics.Test {

    using Plisky.Diagnostics;
    using System.Threading;
    using Xunit;



    public class BasicInterfaceTests {
        private void WriteASeriesOfMessages(Bilge b) {
            b.Info.Log("Test message");
            b.Verbose.Log("test message");
            b.Error.Log("Test message");
            b.Warning.Log("Test message");
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
            sut.Assert.False(true);

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
            sut.Assert.False(false);
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