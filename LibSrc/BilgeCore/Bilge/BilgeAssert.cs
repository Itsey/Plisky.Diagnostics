using Plisky.Plumbing;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Plisky.Diagnostics {
    public class BilgeAssert : BilgeRoutedBase {
       

      
#if NET452 || NETSTANDARD2_0
        public void NotNull(object what, string msg = null, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            True(what != null, msg, meth, pth, ln);
        }


        public void False(bool what, string msg = null, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            True(!what, msg, meth, pth, ln);
        }


        public void True(bool what, string msg = null, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            if (!what) {
                ActiveRouteMessage(TraceCommandTypes.AssertionFailed, msg, null, meth, pth, ln);
            }
        }

        public void Fail(string msg, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            ActiveRouteMessage(TraceCommandTypes.AssertionFailed, msg, null, meth, pth, ln);
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
    

        internal BilgeAssert(BilgeRouter rt, ConfigSettings cs) : base(rt,cs) {
           
        }
    }
}