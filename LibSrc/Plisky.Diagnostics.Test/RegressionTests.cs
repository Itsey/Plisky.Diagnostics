namespace Plisky.Diagnostics.Test {

    using Plisky.Diagnostics;
    using Plisky.Diagnostics.Listeners;
    using System.Diagnostics;
    using System.Threading;
    using Xunit;

    
    public class TestObject {
        public string stringvalue;
    }

    public class RegressionTests {
        private void WriteASeriesOfMessages(Bilge b) {
            b.Info.Log("Test message");
            b.Verbose.Log("test message");
            b.Error.Log("Test message");
            b.Warning.Log("Test message");
        }




        [Fact]
        [Trait("xunit", "bugfind")]
        public void ObjectDump_WritesObjectContents() {
#if ACTIVEDEBUG
            var ied = new IEmergencyDiagnostics();
#else
            var ied = new MockEmergencyDiagnostics();
#endif
            try {
                ied.Log("START OD");
                TestObject to = new TestObject();
                to.stringvalue = "arfle barfle gloop";
                Bilge sut = TestHelper.GetBilge();
                ied.Log("Before Anything Done");
                var mmh = new MockMessageHandler();
                sut.AddHandler(mmh);

                sut.Info.Dump(to, "context");                
                ied.Log("FLUSH OD");
                sut.Flush();
                ied.Log("FlushAfter OD");
                mmh.SetMustContainForBody("arfle barfle gloop");
                ied.Log("ASSERT OD");
                mmh.AssertAllConditionsMetForAllMessages(true, true);

            } finally {
                ied.Log("END OD");
                ied.Shutdown();
            }
        }


        [Fact]
        [Trait("xunit", "regression")]
        public void MockMessageHandlerStartsEmpty() {
            MockMessageHandler mmh = new MockMessageHandler();
            Assert.True(mmh.TotalMessagesRecieved == 0, "There should be no messages to start with");
        }

        [Fact]
        [Trait("xunit", "regression")]
        public void Default_TraceLevelIsOff() {
            MockMessageHandler mmh = new MockMessageHandler();
            Bilge b = TestHelper.GetBilge(setTrace:false);
            Assert.Equal<TraceLevel>(TraceLevel.Off, b.CurrentTraceLevel);
        }

        [Fact]
        [Trait("xunit", "regression")]
        public void VerboseNotLogged_IfNotVerbose() {
            MockMessageHandler mmh = new MockMessageHandler();
            Bilge b = TestHelper.GetBilge(setTrace: false);
            b.CurrentTraceLevel = TraceLevel.Info;
            b.AddHandler(mmh);
            b.Verbose.Log("Msg");
            Assert.Equal<int>(0, mmh.TotalMessagesRecieved);
        }

        [Fact]
        [Trait("xunit", "regression")]
        public void WriteOnFail_DefaultsFalse() {
            Bilge b = TestHelper.GetBilge();
            Assert.False(b.WriteOnFail, "The write on fail must default to false");
        }

        [Fact]
        [Trait("xunit", "regression")]
        public void QueuedMessagesNotWrittenIfWriteOnFailSet() {
            MockMessageHandler mmh = new MockMessageHandler();
            Bilge b = TestHelper.GetBilge();
            b.AddHandler(mmh);
            b.WriteOnFail = true;
            WriteASeriesOfMessages(b);
            b.Flush();
            Assert.Equal<int>(0, mmh.TotalMessagesRecieved);
        }

        [Trait("xunit", "regression")]
        public void QueuedMessagesWritten_AfterFlush() {
            MockMessageHandler mmh = new MockMessageHandler();
            Bilge sut = TestHelper.GetBilge();
            sut.AddHandler(mmh);
            sut.WriteOnFail = true;
            WriteASeriesOfMessages(sut);

            Assert.Equal<int>(0, mmh.TotalMessagesRecieved);
            sut.TriggerWrite();

            sut.Flush();

            mmh.AssertAllConditionsMetForAllMessages(true);
        }

      

        [Fact]
        [Trait("xunit", "regression")]
        public void InfoNotLogged_IfNotInfo() {
            MockMessageHandler mmh = new MockMessageHandler();
            Bilge b = TestHelper.GetBilge();
            b.CurrentTraceLevel = TraceLevel.Warning;
            b.AddHandler(mmh);

            b.Info.Log("msg");
            b.Verbose.Log("Msg");

            b.Flush();
            Assert.Equal<int>(0, mmh.TotalMessagesRecieved);
        }

        [Fact]
        [Trait("xunit", "regression")]
        public void TraceLevel_Constructor_GetsSet() {
            Bilge b = new Bilge(tl: SourceLevels.Error);
            Assert.Equal<SourceLevels>(SourceLevels.Error, b.ActiveTraceLevel);
        }

        [Fact]
        [Trait("xunit", "regression")]
        public void NothingWrittenWhenTraceOff() {
            MockMessageHandler mmh = new MockMessageHandler();
            var sut = TestHelper.GetBilge();
            sut.CurrentTraceLevel = TraceLevel.Off;
            sut.AddHandler(mmh);

            sut.Info.Log("msg");
            sut.Verbose.Log("Msg");
            sut.Error.Log("Msg");
            sut.Warning.Log("Msg");

            sut.Flush(); 
            Assert.Equal<int>(0, mmh.TotalMessagesRecieved);
        }


        [Fact]
        [Trait("xunit", "regression")]
        public void MultiHandlers_RecieveMultiMessages() {
            var mmh1 = new MockMessageHandler();
            var mmh2 = new MockMessageHandler();
            var sut = TestHelper.GetBilge();
            sut.AddHandler(mmh1);
            sut.AddHandler(mmh2);

            sut.Info.Flow();
            sut.Flush();

            mmh1.AssertAllConditionsMetForAllMessages(true);
            mmh2.AssertAllConditionsMetForAllMessages(true);
        }



        [Fact]
        [Trait("age", "regression")]
        public void Context_FromConstructorReachesMessage() {
            MockMessageHandler mmh = new MockMessageHandler();
            string context = "xxCtxtxx";
            mmh.AssertContextIs(context);
            Bilge sut = TestHelper.GetBilge(context);
            sut.AddHandler(mmh);

            sut.Info.Log("Message should have context");
            sut.Flush();
            Thread.Sleep(1);

            mmh.AssertAllConditionsMetForAllMessages(true);
            
        }

        [Fact]
        [Trait("age", "regression")]
        public void ProcessId_IsCorrectProcessId() {
            int testProcId = Process.GetCurrentProcess().Id;

            MockMessageHandler mmh = new MockMessageHandler();
            mmh.AssertProcessId(testProcId);
            mmh.AssertManagedThreadId(Thread.CurrentThread.ManagedThreadId);
            Bilge sut = TestHelper.GetBilge();

            sut.AddHandler(mmh);
            sut.Info.Log("Message Written");
            sut.Flush();
        

            mmh.AssertAllConditionsMetForAllMessages(true);
        }

        

        [Fact]
        [Trait("xunit", "regression")]
        public void MethodName_MatchesThisMethodName() {
            MockMessageHandler mmh = new MockMessageHandler();
            mmh.SetMethodNameMustContain(nameof(MethodName_MatchesThisMethodName));
            Bilge b = TestHelper.GetBilge();
            b.AddHandler(mmh);
            b.Info.Log("This is a message");
            b.Flush();// (true);
            mmh.AssertAllConditionsMetForAllMessages(true);
        }
    }
}