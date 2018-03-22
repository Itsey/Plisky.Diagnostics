using Plisky.Plumbing;
using System.Runtime.CompilerServices;

namespace Plisky.Diagnostics {
    public class BilgeAssert {
        private BilgeRouter router;
        private string ctxt;

        private void ActiveRouteMessage(MessageMetadata mmd) {
            // Some methods pass all this context around so the can call this directly.  All of the shared routing info should be done her
            // with the other overload only used to call this one.

                router.PrepareMetaData(mmd, ctxt);
                router.QueueMessage(mmd);

        }
        private void ActiveRouteMessage(TraceCommandTypes tct, string messageBody, string furtherInfo = null, string methodName = null, string fileName = null, int lineNumber = 0) {

            MessageMetadata mmd = new MessageMetadata() {
                CommandType = tct,
                MethodName = methodName,
                FileName = fileName,
                LineNumber = lineNumber.ToString(),
                Body = messageBody,
                FurtherDetails = furtherInfo
            };
            ActiveRouteMessage(mmd);


        }

#if NET452 || NETSTANDARD2_0
        public void True(bool what,string msg=null, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            if (!what) {
                ActiveRouteMessage(TraceCommandTypes.AssertionFailed, msg, null, meth, pth, ln);
            }
        }

        public void Fail(string msg, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            ActiveRouteMessage(TraceCommandTypes.AssertionFailed, msg, null, meth, pth, ln);
        }
#else

         public void True(bool what,string msg=null, string meth = null,  string pth = null, int ln = 0) {
            if (!what) {
                ActiveRouteMessage(TraceCommandTypes.AssertionFailed, msg, null, meth, pth, ln);
            }
        }

        public void Fail(string msg, string meth = null,  string pth = null, int ln = 0) {
            ActiveRouteMessage(TraceCommandTypes.AssertionFailed, msg, null, meth, pth, ln);
        }

#endif

        internal BilgeAssert(BilgeRouter rt, ConfigSettings cs, string ctxt) {
            router = rt;
        }
    }
}