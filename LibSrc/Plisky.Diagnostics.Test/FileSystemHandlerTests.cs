using Plisky.Diagnostics.Copy;
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
        [Trait(Traits.Age, Traits.Regression)]
        public void SimpleHandler_WellKnownFile() {
            string wellKnownLogfileName = "bilgedefault.log";
            string expectedDestination = Path.Combine(Path.GetTempPath(), wellKnownLogfileName);
            var pfsh = new SimpleTraceFileHandler();
            Assert.Equal(expectedDestination, pfsh.TraceFilename);
        }


        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        public void SimpleHandler_AppendsDate() {
            string wellKnownLogfileName = "bilgedefault.log";
            string expectedDestination = Path.Combine(Path.GetTempPath(), wellKnownLogfileName);
            var pfsh = new SimpleTraceFileHandler(overwriteEachTime:false);
            Assert.NotEqual(expectedDestination, pfsh.TraceFilename);
        }

      
    }
}