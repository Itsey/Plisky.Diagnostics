namespace Plisky.Diagnostics.Test {

    using Plisky.Diagnostics;
    using Plisky.Diagnostics.Copy;
    using Plisky.Diagnostics.Listeners;
    using System;
    using System.Diagnostics;
    using System.Linq;
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



        [Theory(DisplayName = nameof(WriteMessage_ArrivesAsMessage))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        [InlineData(TraceCommandTypes.LogMessage)]
        [InlineData(TraceCommandTypes.LogMessageVerb)]
        [InlineData(TraceCommandTypes.ErrorMsg)]
        [InlineData(TraceCommandTypes.WarningMsg)]
        public void WriteMessage_ArrivesAsMessage(TraceCommandTypes testCase) {
            var sut = TestHelper.GetBilgeAndClearDown();
            sut.ActiveTraceLevel = SourceLevels.Verbose;
            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);

            
            WriteCorrectTypeOfMessage(sut, testCase);
            sut.Flush();
            Thread.Sleep(10);

            var msg = mmh.GetMostRecentMessage();

            Assert.NotNull(msg);
            Assert.Equal(testCase, msg.CommandType);
        }

        private void WriteCorrectTypeOfMessage(Bilge sut, TraceCommandTypes testCase) {
            switch (testCase) {
                case TraceCommandTypes.LogMessage: sut.Info.Log("Test"); break;
                case TraceCommandTypes.LogMessageVerb: sut.Verbose.Log("Test"); break;
                case TraceCommandTypes.TraceMessageIn: sut.Info.EnterSection("Test"); break;
                case TraceCommandTypes.ErrorMsg: sut.Error.Log("Test"); break;
                case TraceCommandTypes.WarningMsg: sut.Warning.Log("Test"); break;

                case TraceCommandTypes.LogMessageMini:
                case TraceCommandTypes.InternalMsg:
                case TraceCommandTypes.TraceMessageOut:
                case TraceCommandTypes.TraceMessage:
                case TraceCommandTypes.AssertionFailed:
                case TraceCommandTypes.MoreInfo:
                case TraceCommandTypes.CommandOnly:
                case TraceCommandTypes.ExceptionBlock:
                case TraceCommandTypes.ExceptionData:
                case TraceCommandTypes.ExcStart:
                case TraceCommandTypes.ExcEnd:
                case TraceCommandTypes.SectionStart:
                case TraceCommandTypes.SectionEnd:
                case TraceCommandTypes.ResourceEat:
                case TraceCommandTypes.ResourcePuke:
                case TraceCommandTypes.ResourceCount:
                case TraceCommandTypes.Standard:
                case TraceCommandTypes.CommandXML:
                case TraceCommandTypes.Custom:
                case TraceCommandTypes.Alert:
                case TraceCommandTypes.Unknown:
                default:
                    throw new NotImplementedException();
                    
            }
        }

        [Fact(DisplayName = nameof(GetHandlers_DefaultReturnsAll))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void GetHandlers_DefaultReturnsAll() {
            _ = TestHelper.GetBilgeAndClearDown();

            Bilge.AddHandler(new MockMessageHandler());
            Bilge.AddHandler(new MockMessageHandler());
            Bilge.AddHandler(new MockMessageHandler());
            Bilge.AddHandler(new MockMessageHandler());

            Assert.Equal(4, Bilge.GetHandlers().Count());
        }

        [Fact(DisplayName = nameof(GetHandlers_FilterReturnsNamed))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void GetHandlers_FilterReturnsNamed() {
            _ = TestHelper.GetBilgeAndClearDown();

            Bilge.AddHandler(new MockMessageHandler("arfle"));
            Bilge.AddHandler(new MockMessageHandler("barfle"));
            var count = Bilge.GetHandlers("arf*").Count();

            Assert.Equal(1, count);
        }



        [Fact(DisplayName = nameof(AddHandler_DoesAddHandler))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void AddHandler_DoesAddHandler() {
            _ = TestHelper.GetBilgeAndClearDown();

            bool worked = Bilge.AddHandler(new MockMessageHandler());
            var count = Bilge.GetHandlers().Count();

            Assert.True(worked);
            Assert.Equal(1, count);
        }


        [Fact(DisplayName = nameof(AddHandler_DuplicateAddsTwoHandlers))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void AddHandler_DuplicateAddsTwoHandlers() {
            _ = TestHelper.GetBilgeAndClearDown();

            bool worked = Bilge.AddHandler(new MockMessageHandler());
            worked &= Bilge.AddHandler(new MockMessageHandler());

            Assert.True(worked);
            var ct = Bilge.GetHandlers().Count();
            Assert.Equal(2, ct);
        }


        [Fact(DisplayName = nameof(AddHandler_DuplicateByNameFailsOnSecond))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void AddHandler_DuplicateByNameFailsOnSecond() {
            _ = TestHelper.GetBilgeAndClearDown();

            Bilge.AddHandler(new MockMessageHandler());
            Bilge.AddHandler(new MockMessageHandler());

            var ct = Bilge.GetHandlers().Count();
            Assert.Equal(2, Bilge.GetHandlers().Count());
        }

        [Fact(DisplayName = nameof(AddHandler_SingleType_DoesNotAddSecond))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void AddHandler_SingleType_DoesNotAddSecond() {
            _ = TestHelper.GetBilgeAndClearDown();

            Bilge.AddHandler(new MockMessageHandler(), HandlerAddOptions.SingleType);
            Bilge.AddHandler(new MockMessageHandler(), HandlerAddOptions.SingleType);
            var ct = Bilge.GetHandlers().Count();

            Assert.Equal(1, ct);
        }

        [Fact(DisplayName = nameof(AddHandler_SingleType_AddsDifferentTypes))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void AddHandler_SingleType_AddsDifferentTypes() {
            _ = TestHelper.GetBilgeAndClearDown();

            Bilge.AddHandler(new MockMessageHandler(), HandlerAddOptions.SingleType);
            Bilge.AddHandler(new InMemoryHandler(), HandlerAddOptions.SingleType);
            var ct = Bilge.GetHandlers().Count();

            Assert.Equal(2, ct);
        }

        [Fact(DisplayName = nameof(AddHandler_SingleName_AddsDifferentNames))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void AddHandler_SingleName_AddsDifferentNames() {
            _ = TestHelper.GetBilgeAndClearDown();

            Bilge.AddHandler(new MockMessageHandler("arfle"), HandlerAddOptions.SingleName);
            Bilge.AddHandler(new MockMessageHandler("barfle"), HandlerAddOptions.SingleName);
            var ct = Bilge.GetHandlers().Count();

            Assert.Equal(2, ct);
        }

        [Fact(DisplayName = nameof(AddHandler_SingleName_DoesNotAddSecondName))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void AddHandler_SingleName_DoesNotAddSecondName() {
            _ = TestHelper.GetBilgeAndClearDown();

            Bilge.AddHandler(new MockMessageHandler("arfle"), HandlerAddOptions.SingleName);
            Bilge.AddHandler(new MockMessageHandler("arfle"), HandlerAddOptions.SingleName);
            var ct = Bilge.GetHandlers().Count();

            Assert.Equal(1, ct);
        }



        [Fact(DisplayName = nameof(DirectWrite_IsPossible))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void DirectWrite_IsPossible() {
            Bilge sut = TestHelper.GetBilgeAndClearDown();
            sut.DisableMessageBatching();
            sut.ActiveTraceLevel = SourceLevels.Verbose;

            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);

            sut.Direct.Write("DirectMessage", "DirectFurther");
            sut.Flush();

            Assert.Equal(1, mmh.TotalMessagesRecieved);

        }


        [Fact(DisplayName = nameof(MessageBatching_Works_Default1))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void MessageBatching_Works_Default1() {
            Bilge sut = TestHelper.GetBilgeAndClearDown();
            sut.ActiveTraceLevel = SourceLevels.Information;
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

            Bilge sut = TestHelper.GetBilgeAndClearDown();

            sut.SetMessageBatching(MESSAGE_BATCHSIZE, 500000);

            sut.CurrentTraceLevel = System.Diagnostics.TraceLevel.Info;
            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);

            for (int i = 0; i < 100; i++) {
                sut.Info.Log("Dummy Message");


                if (i % 25 == 0) {
                    //Thread.Sleep(100);
                    // The flush forces the write, this is needed otherwise it bombs through
                    // too fast for more than one write to the handler to occur.
                    sut.Flush();
                }

                if (mmh.TotalMessagesRecieved > 0) {
                    // Any time that we get a batch it must be at least MESSAGE_BATCHSIZE msgs.
                    Assert.True(mmh.LastMessageBatchSize >= MESSAGE_BATCHSIZE, $"Batch Size NotBig Enough at {i} batch Size {mmh.LastMessageBatchSize}");
                }


            }


        }



        [Fact(DisplayName = nameof(MessageBatching_Works_Timed))]
        /// [Trait(Traits.Age, Traits.Fresh)]
        //[Trait(Traits.Style, Traits.Unit)]
        public void MessageBatching_Works_Timed() {
            const int MESSAGE_BATCHSIZE = 10000;

            Bilge sut = TestHelper.GetBilgeAndClearDown();

            sut.SetMessageBatching(MESSAGE_BATCHSIZE, 250);

            sut.CurrentTraceLevel = System.Diagnostics.TraceLevel.Info;
            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);

            sut.Info.Log("Dummy Message");

            Stopwatch timeSoFar = new Stopwatch();
            timeSoFar.Start();

            bool writesFound = false;

            while (timeSoFar.Elapsed.TotalMilliseconds < 750) {
                // This is not particularly precise because of threading and guarantees so we are using some generous margins for error.
                // With the write time of not less than 250 we shouldnt see any writes for the first 175 MS.  If we do then its a test fail.
                // Similarly if we reach 750 ms and havent seen any writes thats a test fail.

                if (timeSoFar.ElapsedMilliseconds < 175) {
                    Assert.Equal(0, mmh.TotalMessagesRecieved);
                } else {
                    if (mmh.TotalMessagesRecieved > 0) {
                        writesFound = true;
                        break;
                    }
                }
                if (timeSoFar.ElapsedMilliseconds > 350) {
                    sut.Flush();
                }
            }

            if (!writesFound) {
                throw new InvalidOperationException("The writes never hit the listener");
            }

        }


        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        public void Enter_WritesMethodName() {
            Bilge sut = TestHelper.GetBilgeAndClearDown();
            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);
            sut.Info.E();

            sut.Flush();

            mmh.SetMustContainForBody(nameof(Enter_WritesMethodName));

            // E generates more than one message, therefore we have to check that one of the messages had the name in it.
            mmh.AssertAllConditionsMetForAllMessages(true, true);

        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        public void Exit_WritesMethodName() {

            var mmh = new MockMessageHandler();
            mmh.SetMethodNameMustContain(nameof(Exit_WritesMethodName));
            Bilge sut = TestHelper.GetBilgeAndClearDown();
            sut.ActiveTraceLevel = SourceLevels.Verbose;
            sut.AddHandler(mmh);
            sut.Info.X();

            sut.Flush();

            // X generates more than one message, therefore we have to check that one of the messages had the name in it.
            mmh.AssertAllConditionsMetForAllMessages(true, true);

        }


        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        public void Bilge_EnterSection_TracesSection() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge(tl: SourceLevels.Verbose);
            sut.AddHandler(mmh);
            mmh.SetMethodNameMustContain("monkeyfish");
            sut.Info.EnterSection("random sectiion", "monkeyfish");
            sut.Flush();

            mmh.AssertAllConditionsMetForAllMessages(true, true);
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        public void Bilge_LeaveSection_TracesSection() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge(tl: SourceLevels.Verbose);

            sut.AddHandler(mmh);

            mmh.SetMethodNameMustContain("bannanaball");
            sut.Info.LeaveSection("bannanaball");


            sut.Flush();

            mmh.AssertAllConditionsMetForAllMessages(true, true);

        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        public void Assert_True_DoesNothingIfTrue() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge();
            sut.AddHandler(mmh);

            sut.Assert.True(true);
            Assert.Equal(0, mmh.AssertionMessageCount);
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        public void Assert_True_DoesFailsIfFalse() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge();
            sut.AddHandler(mmh);

            sut.Assert.True(false);

            sut.Flush();


            Assert.True(mmh.AssertionMessageCount > 0);
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        public void Assert_False_FailsIfTrue() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge();
            sut.AddHandler(mmh);
            sut.Assert.True(false);

            sut.Flush();

            Assert.True(mmh.AssertionMessageCount > 0);
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        public void Assert_False_DoesNothingIfFalse() {
            var mmh = new MockMessageHandler();
            Bilge sut = new Bilge();
            sut.AddHandler(mmh);
            sut.Assert.True(true);
            Assert.Equal(0, mmh.AssertionMessageCount);
        }



        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        public void Flow_WritesMethodNameToMessage() {
            MockMessageHandler mmh = new MockMessageHandler();
            mmh.SetMethodNameMustContain(nameof(Flow_WritesMethodNameToMessage));
            mmh.SetMustContainForBody(nameof(Flow_WritesMethodNameToMessage));
            var sut = TestHelper.GetBilgeAndClearDown();
            sut.ActiveTraceLevel = SourceLevels.Verbose;
            sut.AddHandler(mmh);

            sut.Info.Flow();
            sut.Flush();

            mmh.AssertAllConditionsMetForAllMessages(true);
        }




    }

}