using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Plisky.Diagnostics {


    public class BilgeDirect {
        private bool IsWriting = true;
        public string Context { get; set; }


        private BilgeRouter router;

#if NET452 || NETSTANDARD2_0
        public void Write(string body, string further, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
#else
        public void Write(string body, string further, string meth = null,  string pth = null, int ln = 0) {
#endif
            MessageMetadata mmd = new MessageMetadata(meth, pth, ln);
            mmd.CommandType = TraceCommandTypes.Custom;
            mmd.Body = body;
            mmd.FurtherDetails = further;
            mmd.Body = body;
            

            if (IsWriting) {
                router.PrepareMetaData(mmd, config.metaContexts);
                router.QueueMessage(mmd);
            }
        }

        protected ConfigSettings config;

       
        
        internal BilgeDirect(BilgeRouter r, ConfigSettings activeConfig) {
            router = r;
            config = activeConfig;
        }
    }


}
