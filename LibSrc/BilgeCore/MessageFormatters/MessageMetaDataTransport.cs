using System;
using System.Collections.Generic;
using System.Text;

namespace Plisky.Diagnostics {
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Class is used to transport messagemetadata, using shortened names and so on to reduce the amount of data to as little as possible, this class should not
    /// be used unless its to turn a formatted message from the FlimFlamV2 Formatter class back into a message meta data.
    /// </summary>
    public class MessageMetaDataTransport {

        public string m { get; set; }  // message
        
        public string s { get; set; }  // secondary
        
        public string mt { get; set; }  // commmand type
        public string c { get; set; }  // classname
        public string l { get; set; }   // linenumber

        public string p { get; set; } // process name

        public string t { get; set; } // OSthreadId
        public string nt { get; set; } // NetthreadId

        public string mn { get; set; } // MethodName

        public string man { get; set; } // MachineName

        public string al { get; set; } // alt loc

        public string md { get; set; } // module / filename

        public string uq { get; set; }  //uniqueness identifier
        public string v { get; set; } // messageversion

        public void FromMessageMetaData(MessageMetadata source) {

        }

        public TraceCommandTypes GetCommandType() {
            return TraceCommands.StringToTraceCommand(mt);
        }
    }

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
