namespace Plisky.Diagnostics.Test {

    using Plisky.Diagnostics;
    using System;
    using System.Diagnostics;
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



        [Fact(DisplayName = nameof(DirectWrite_IsPossible))]
        //[Trait(Traits.Age, Traits.Fresh)]
        //[Trait(Traits.Style, Traits.Unit)]
        public void DirectWrite_IsPossible() {
            Bilge sut = TestHelper.GetBilge();
            sut.DisableMessageBatching();
            sut.CurrentTraceLevel = TraceLevel.Verbose;

            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);

            sut.Direct.Write("DirectMessage", "DirectFurther");
            sut.Flush();

            Assert.Equal(1, mmh.TotalMessagesRecieved);

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

            sut.SetMessageBatching(MESSAGE_BATCHSIZE, 500000);

            sut.CurrentTraceLevel = System.Diagnostics.TraceLevel.Info;
            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);

            for (int i = 0; i < 100; i++) {
                sut.Info.Log("Dummy Message");

                
                if (i%25==0) {
                    //Thread.Sleep(100);
                    // The flush forces the write, this is needed otherwise it bombs through
                    // too fast for more than one write to the handler to occur.
                    sut.Flush();
                }

                if (mmh.TotalMessagesRecieved > 0) {
                    // Any time that we get a batch it must be at least MESSAGE_BATCHSIZE msgs.
                    Assert.True(mmh.LastMessageBatchSize >= MESSAGE_BATCHSIZE,$"Batch Size NotBig Enough at {i} batch Size {mmh.LastMessageBatchSize}");
                }

        
            }
            

        }



        [Fact(DisplayName = nameof(MessageBatching_Works_Timed))]
        /// [Trait(Traits.Age, Traits.Fresh)]
        //[Trait(Traits.Style, Traits.Unit)]
        public void MessageBatching_Works_Timed() {
            const int MESSAGE_BATCHSIZE = 10000;

            Bilge sut = TestHelper.GetBilge();

            sut.SetMessageBatching(MESSAGE_BATCHSIZE, 250);

            sut.CurrentTraceLevel = System.Diagnostics.TraceLevel.Info;
            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);

            sut.Info.Log("Dummy Message");

            Stopwatch timeSoFar = new Stopwatch();
            timeSoFar.Start();

            bool writesFound = false;

            while(timeSoFar.Elapsed.TotalMilliseconds<750) {
                // This is not particularly precise because of threading and guarantees so we are using some generous margins for error.
                // With the write time of not less than 250 we shouldnt see any writes for the first 175 MS.  If we do then its a test fail.
                // Similarly if we reach 750 ms and havent seen any writes thats a test fail.

                if (timeSoFar.ElapsedMilliseconds<175) {
                    Assert.Equal(0, mmh.TotalMessagesRecieved);
                } else {
                    if (mmh.TotalMessagesRecieved>0) {
                        writesFound = true;
                        break;
                    }
                }
                if (timeSoFar.ElapsedMilliseconds>350) {
                    sut.Flush();
                }
            }
            
            if (!writesFound) {
                throw new InvalidOperationException("The writes never hit the listener");
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
           
            var mmh = new MockMessageHandler();
            mmh.SetMethodNameMustContain(nameof(Exit_WritesMethodName));
            Bilge sut = TestHelper.GetBilge();
            sut.AddHandler(mmh);
            sut.Info.X();

            sut.Flush();

            // X generates more than one message, therefore we have to check that one of the messages had the name in it.
            mmh.AssertAllConditionsMetForAllMessages(true, true);

        }


        [Fact]
        [Trait("xunit", "regression")]
        public void Bilge_EnterSection_TracesSection() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge();
            sut.AddHandler(mmh);
            mmh.SetMethodNameMustContain("monkeyfish");
            sut.Info.EnterSection("random sectiion","monkeyfish");
            sut.Flush();
          

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

     
            sut.Flush();

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
            mmh.SetMethodNameMustContain(nameof(Flow_WritesMethodNameToMessage));
            mmh.SetMustContainForBody(nameof(Flow_WritesMethodNameToMessage));
            var sut = TestHelper.GetBilge();
            sut.AddHandler(mmh);

            sut.Info.Flow();
            sut.Flush();

            mmh.AssertAllConditionsMetForAllMessages(true);
        }




    }

}