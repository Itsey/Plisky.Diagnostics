using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Plisky.Diagnostics.Test {
    public class TestHelper {

        public static MessageMetadata[] GetMessageMetaData(int howMany = 1) {
            List<MessageMetadata> result = new List<MessageMetadata>();

            for (int i = 0; i < howMany; i++) {
                var r = new MessageMetadata() {
                    Body = "Body",
                    ClassName = "Class",
                    CommandType = TraceCommandTypes.LogMessage,
                    FileName = @"C:\temp\temp\filename.txt",
                    FurtherDetails = " This is further details",
                    Context = "this is the content",
                    MachineName = "AMACHINENAME123",
                    MethodName = "AMethod_1234",
                    LineNumber = "10",
                    ProcessId = "1234565",
                    NetThreadId = "12abc",
                    OSThreadId = "1234"
                };
                result.Add(r);
            }
            return result.ToArray();
        }

        public static Bilge GetBilgeAndClearDown(string context = null, bool setTrace = true) {
            Bilge result;
            if (context != null) {
                result = new Bilge(context, resetDefaults: true);
            } else {
                result = new Bilge(resetDefaults: true);
            }

#if DEBUG
            Assert.True(result.IsCleanInitialise(), "Unclean!");
#endif
            if (setTrace) {
                result.CurrentTraceLevel = System.Diagnostics.TraceLevel.Info;
            }
            return result;
        }

    }
}
