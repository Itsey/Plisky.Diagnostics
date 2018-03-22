using Plisky.Diagnostics.Listeners;
using Plisky.Helpers;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Plisky.Diagnostics.Test {

    public class FileSystemHandlerTests {
        private UnitTestHelper uth = new UnitTestHelper();


        [Fact]
        [Trait("xunit", "fresh")]
        public void SimpleHandler_WellKnownFile() {
            string wellKnownLogfileName = "bilgedefault.log";
            string expectedDestination = Path.Combine(Path.GetTempPath(), wellKnownLogfileName);
            var pfsh = new SimpleTraceFileHandler();
            Assert.Equal(expectedDestination, pfsh.TraceFilename);
        }


        [Fact]
        [Trait("xunit", "fresh")]
        public void SimpleHandler_AppendsDate() {
            string wellKnownLogfileName = "bilgedefault.log";
            string expectedDestination = Path.Combine(Path.GetTempPath(), wellKnownLogfileName);
            var pfsh = new SimpleTraceFileHandler(overwriteEachTime:false);
            Assert.NotEqual(expectedDestination, pfsh.TraceFilename);
        }






        [Fact]
        [Trait("xunit", "bugfind")]
        public void BasicFileWriting() {
#if ACTIVEDEBUG
            var ied = new IEmergencyDiagnostics();
#else
            var ied = new MockEmergencyDiagnostics();
#endif


            ied.Log("Starting BAsic File Writing");

            try {
                string basePath = Path.GetTempPath();
                string expectedFname = Path.Combine(basePath, "bilgedefault.log");

                if (File.Exists(expectedFname)) {
                    File.Delete(expectedFname);
                }
                ied.Log("Getting Bilge, Clearodwn expected");
                Bilge sut = TestHelper.GetBilge();
                ied.Log("Clear down done");

                var pfsh = new SimpleTraceFileHandler(basePath);
                sut.AddHandler(pfsh);
                ied.Log("SimpleFileListener added");

                sut.Info.Log("This is the text to write anti");
                sut.Info.Log("This is the text to write manti");
                sut.Info.Log("This is the text to write panti");
                sut.Info.Log("This is the text to write hose");

                Thread.Sleep(100);

                ied.Log("Flush occured");

                sut.FlushAndShutdown();

                Assert.True(File.Exists(expectedFname), "Well known filename should be created on trace");

                string s = File.ReadAllText(expectedFname);
                
                File.Delete(expectedFname);

                ied.Log("STringAll >> " + s ?? "null");

                Assert.True(uth.StringContainsAll(s, new string[] { "anti", "manti", "panti", "hose" }), "The strings should be logged ot the file");
            } finally {

                ied.Shutdown();

            }
        }

        [Fact(Skip ="fails, no idea why")]
        [Trait("xunit", "exploratory")]
        public void HowDoesAsWork() {
            string basePath = Path.GetTempPath();
            var sft = new SimpleTraceFileHandler(basePath);
            var sf2 = new TCPHandler("", 1234);
            var x = sft as IDisposable;
            var y = sf2 as IDisposable;
            Assert.NotNull(x);
            Assert.Null(y);
        }
    }
}