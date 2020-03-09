
namespace Plisky.Diagnostics {
    using Plisky.Plumbing;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;


    public abstract class BilgeConditionalRoutedBase : BilgeRoutedBase {
        protected SourceLevels activeTraceLevel;

        /// <summary>
        /// Determines whether this conditional router is actually writing to the stream.
        /// </summary>
        public bool IsWriting { get; set; }


        public string ContextCache {
            get {
                return sets.InstanceContext;
            }
        }

        protected override void ActiveRouteMessage(MessageMetadata mmd) {
            if (IsWriting) {
                base.ActiveRouteMessage(mmd);
            }
        }

        public BilgeConditionalRoutedBase(BilgeRouter rt, ConfigSettings cs, SourceLevels yourTraceLevel) : base(rt, cs) {
            activeTraceLevel = yourTraceLevel;
        }
    }

    public abstract class BilgeRoutedBase {
        protected BilgeRouter router;
        protected ConfigSettings sets;

        public BilgeRoutedBase(BilgeRouter rt, ConfigSettings cs) {
            router = rt ?? throw new ArgumentNullException(nameof(rt));
            sets = cs ?? throw new ArgumentNullException(nameof(cs));
        }



        protected virtual void ActiveRouteMessage(MessageMetadata mmd) {
#if DEBUG
            if (mmd == null) { throw new ArgumentNullException(nameof(mmd)); }
            if (sets == null) { throw new InvalidOperationException("Settings must be set before this call. DevFault");  }
            if (sets.metaContexts == null) { throw new InvalidOperationException("Settings.MetaContexts must be set before this call. DevFault"); }
#endif

            // Some methods pass all this context around so the can call this directly.  All of the shared routing info should be done her
            // with the other overload only used to call this one.

            router.PrepareMetaData(mmd, sets.metaContexts);
            router.QueueMessage(mmd);

        }

        protected void ActiveRouteMessage(TraceCommandTypes tct, string messageBody, string furtherInfo = null, string methodName = null, string fileName = null, int lineNumber = 0) {

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
    }
}
