using Plisky.Plumbing;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Plisky.Diagnostics {
    public class BilgeAssert {
        private BilgeRouter router;

        private void ActiveRouteMessage(MessageMetadata mmd, Dictionary<string,string> metaContext) {
            // Some methods pass all this context around so the can call this directly.  All of the shared routing info should be done her
            // with the other overload only used to call this one.

                router.PrepareMetaData(mmd, metaContext);
                router.QueueMessage(mmd);

        }
        private void ActiveRouteMessage(TraceCommandTypes tct, Dictionary<string, string> mc, string messageBody, string furtherInfo = null, string methodName = null, string fileName = null, int lineNumber = 0) {

            MessageMetadata mmd = new MessageMetadata() {
                CommandType = tct,
                MethodName = methodName,
                FileName = fileName,
                LineNumber = lineNumber.ToString(),
                Body = messageBody,
                FurtherDetails = furtherInfo
            };
            ActiveRouteMessage(mmd, mc);


        }
#if NET452 || NETSTANDARD2_0
        public void NotNull(object what, string msg = null, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            True(what != null, msg, meth, pth, ln);
        }


        public void False(bool what, string msg= null, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            True(!what, msg, meth, pth, ln);
        }


        public void True(bool what,string msg=null, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            if (!what) {
                ActiveRouteMessage(TraceCommandTypes.AssertionFailed, null ,msg, null, meth, pth, ln);
            }
        }

        public void Fail(string msg, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            ActiveRouteMessage(TraceCommandTypes.AssertionFailed,null, msg, null, meth, pth, ln);
        }
#else

         public void NotNull(object what, string msg = null, string meth = null,  string pth = null, int ln = 0) {
            True(what != null, msg, meth, pth, ln);
        }


        public void False(bool what, string msg= null, string meth = null,  string pth = null, int ln = 0) {
            True(!what, msg, meth, pth, ln);
        }

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