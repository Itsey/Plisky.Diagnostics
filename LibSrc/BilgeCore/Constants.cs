/* The trace constants are literal strings that are included in the output debug string.  Most of the debug string transference
 * methods - such as the win32 outputdebugstring api will transmit a single string only therefore the debug string is made up
 * of the text that is to be output with additional information on the front that is specific to the viewer.  In this way not only
 can messages be attributed to their application and thread but also viewer commands such as clearscreen and line can be sent */

namespace Plisky.Plumbing {

    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Trace Command Types - represent all of the possible command types that can be issued as a trace command.
    /// </summary>
    [Flags]
    public enum TraceCommandTypes : uint {
        LogMessage = 0x00000001,
        LogMessageVerb = 0x00000002,
        LogMessageMini = 0x00000004,
        InternalMsg = 0x00000008,
        TraceMessageIn = 0x00000010,
        TraceMessageOut = 0x00000020,
        TraceMessage = 0x00000040,
        AssertionFailed = 0x00000080,
        MoreInfo = 0x00000100,
        CommandOnly = 0x00000200,
        ErrorMsg = 0x00000400,
        WarningMsg = 0x00000800,
        ExceptionBlock = 0x00001000,  // used for exception type flag
        ExceptionData = 0x00002000,
        ExcStart = 0x00004000,
        ExcEnd = 0x00008000,
        SectionStart = 0x00010000,
        SectionEnd = 0x00020000,
        ResourceEat = 0x00040000,
        ResourcePuke = 0x00080000,
        ResourceCount = 0x00100000,
        Standard = 0x00200000,
        CommandXML = 0x00400000,
        Custom = 0x00800000,
        Unknown = 0x00000000
    };

    /// <summary>
    /// Known Commands represent commands that can be sent directly to the viewer
    /// </summary>
    public enum KnownCommand {

        /// <summary>
        /// Tell the viewer to increase the indent level on the following outputs
        /// </summary>
        IncreaseIndent,

        /// <summary>
        /// Tell the viewer to decrease the indent level on the following outputs
        /// </summary>
        DecreaseIndent,

        /// <summary>
        /// Tell the viewer to remove all curent trace data from its store
        /// </summary>
        PurgeAll,

        /// <summary>
        /// Tell the viewer to remove all current trace data for thsi current process from its store
        /// </summary>
        PurgeCurrent,

        /// <summary>
        /// Tell the viewer not to leave a line between this entry and the next entry.
        /// </summary>
        DoNotLeaveLine,

        /// <summary>
        /// Tells the viewer to start ignoring all messages after this one
        /// </summary>
        StartFilteringEvents,

        /// <summary>
        /// Tells the viewer to stop ignoring all messages after this one
        /// </summary>
        StopFilteringEvents
    };

    /// <summary>
    /// Trace commands represents further information about the trace commands themselvers
    /// </summary>
    public static class TraceCommands {

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Long switch statement but very simple to read")]
        public static string TraceCommandToString(TraceCommandTypes theCommand) {
            switch (theCommand) {
                case TraceCommandTypes.LogMessage: return Constants.MSGFMT_LOG;
                case TraceCommandTypes.LogMessageVerb: return Constants.MSGFMT_LOGVERBOSE;
                case TraceCommandTypes.InternalMsg: return Constants.INTERNALMSG;
                case TraceCommandTypes.TraceMessageIn: return Constants.TRACEMESSAGEIN;
                case TraceCommandTypes.TraceMessageOut: return Constants.TRACEMESSAGEOUT;
                case TraceCommandTypes.AssertionFailed: return Constants.ASSERTIONFAILED;
                case TraceCommandTypes.MoreInfo: return Constants.MOREINFO;
                case TraceCommandTypes.CommandOnly: return Constants.COMMANDONLY;
                case TraceCommandTypes.ErrorMsg: return Constants.ERRORMSG;
                case TraceCommandTypes.WarningMsg: return Constants.WARNINGMSG;
                case TraceCommandTypes.ExceptionData: return Constants.EXCEPTIONDATA;
                case TraceCommandTypes.ExceptionBlock: return Constants.EXCEPTIONBLOCK;
                case TraceCommandTypes.ExcStart: return Constants.EXCSTART;
                case TraceCommandTypes.ExcEnd: return Constants.EXCEND;
                case TraceCommandTypes.SectionStart: return Constants.SECTIONSTART;
                case TraceCommandTypes.SectionEnd: return Constants.SECTIONEND;
                case TraceCommandTypes.ResourceEat: return Constants.RESOURCEEAT;
                case TraceCommandTypes.ResourcePuke: return Constants.RESOURCEPUKE;
                case TraceCommandTypes.ResourceCount: return Constants.RESOURCECOUNT;
                case TraceCommandTypes.CommandXML: return Constants.MSGFMT_XMLCOMMAND;
                case TraceCommandTypes.Custom: return Constants.MSGFMT_CUSTOM;
                
            }

            throw new ArgumentException("Unreachable Code, the value of the parameter is invalid, this code should not be executed.", "theCommand");
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Long switch statement but very simple to read")]
        internal static TraceCommandTypes StringToTraceCommand(string theCmdText) {
            switch (theCmdText) {
                case Constants.MSGFMT_LOG: return TraceCommandTypes.LogMessage;
                case Constants.MSGFMT_LOGVERBOSE: return TraceCommandTypes.LogMessageVerb;
                case Constants.INTERNALMSG: return TraceCommandTypes.InternalMsg;
                case Constants.TRACEMESSAGEIN: return TraceCommandTypes.TraceMessageIn;
                case Constants.TRACEMESSAGEOUT: return TraceCommandTypes.TraceMessageOut;
                case Constants.ASSERTIONFAILED: return TraceCommandTypes.AssertionFailed;
                case Constants.MOREINFO: return TraceCommandTypes.MoreInfo;
                case Constants.COMMANDONLY: return TraceCommandTypes.CommandOnly;
                case Constants.MSGFMT_XMLCOMMAND: return TraceCommandTypes.CommandXML;
                case Constants.ERRORMSG: return TraceCommandTypes.ErrorMsg;
                case Constants.WARNINGMSG: return TraceCommandTypes.WarningMsg;
                case Constants.EXCEPTIONBLOCK: return TraceCommandTypes.ExceptionBlock;
                case Constants.EXCEPTIONDATA: return TraceCommandTypes.ExceptionData;
                case Constants.EXCSTART: return TraceCommandTypes.ExcStart;
                case Constants.EXCEND: return TraceCommandTypes.ExcEnd;
                case Constants.SECTIONSTART: return TraceCommandTypes.SectionStart;
                case Constants.SECTIONEND: return TraceCommandTypes.SectionEnd;
                case Constants.RESOURCEEAT: return TraceCommandTypes.ResourceEat;
                case Constants.RESOURCEPUKE: return TraceCommandTypes.ResourcePuke;
                case Constants.RESOURCECOUNT: return TraceCommandTypes.ResourceCount;
                case Constants.MSGFMT_CUSTOM: return TraceCommandTypes.Custom;
            }

            throw new ArgumentException("Unreachable Code, the value of the parameter is invalid, this code should not be executed.", "theCmdText");
        }

        /// <summary>
        /// This will take the trace command enum and turn each of the valid entries into a readable string that is suitable
        /// to be printed on the screen or displayed to the user.
        /// </summary>
        /// <param name="tct">The trace command types enum selected and valid value</param>
        /// <returns>string representing that value</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Long switch statement but very simple to read")]
        internal static string TraceCommandToReadableString(TraceCommandTypes tct) {
            switch (tct) {
                case TraceCommandTypes.AssertionFailed: return "Assertion";
                case TraceCommandTypes.CommandOnly: return "Command";
                case TraceCommandTypes.ErrorMsg: return "Error";
                case TraceCommandTypes.ExceptionBlock: return "Exception Block";
                case TraceCommandTypes.ExceptionData: return "Exception";
                case TraceCommandTypes.ExcEnd: return "Exception Inner Start";
                case TraceCommandTypes.ExcStart: return "Exception Inner End";
                case TraceCommandTypes.InternalMsg: return "Internal";
                case TraceCommandTypes.LogMessage: return "Log Message";
                case TraceCommandTypes.LogMessageMini: return "Log Minimal";
                case TraceCommandTypes.LogMessageVerb: return "Log Verbose";
                case TraceCommandTypes.MoreInfo: return "Chain Message";
                case TraceCommandTypes.TraceMessage: return "Trace Message";
                case TraceCommandTypes.TraceMessageIn: return "Trace Enter";
                case TraceCommandTypes.TraceMessageOut: return "Trace Exit";
                case TraceCommandTypes.WarningMsg: return "Warning Message";
                case TraceCommandTypes.SectionStart: return "Section Start";
                case TraceCommandTypes.SectionEnd: return "Section End";
                case TraceCommandTypes.ResourceEat: return "Resource Allocation";
                case TraceCommandTypes.ResourcePuke: return "Resource DeAllocation";
                case TraceCommandTypes.ResourceCount: return "Resource Value Setting";
                case TraceCommandTypes.CommandXML: return "XML Command";
                case TraceCommandTypes.Custom: return "Custom Command";
                default: throw new ArgumentException("Invalid Trace Command Type selected in TraceCommandToReadableString", "tct");
            }
        }

        /// <summary>
        /// This will take the trace command enum and turn each of the valid entries into a readable string that is suitable
        /// to be printed on the screen or displayed to the user.
        /// </summary>
        /// <param name="tcstring">The trace command types enum selected and valid value</param>
        /// <returns>string representing that value</returns>
        internal static TraceCommandTypes ReadableStringToTraceCommand(string tcstring) {
            switch (tcstring) {
                case "Assertion": return TraceCommandTypes.AssertionFailed;
                case "Command": return TraceCommandTypes.CommandOnly;
                case "Error": return TraceCommandTypes.ErrorMsg;
                case "Exception Block": return TraceCommandTypes.ExceptionBlock;
                case "Exception": return TraceCommandTypes.ExceptionData;
                case "Exception Inner Start": return TraceCommandTypes.ExcEnd;
                case "Exception Inner End": return TraceCommandTypes.ExcStart;
                case "Internal": return TraceCommandTypes.InternalMsg;
                case "Log Message": return TraceCommandTypes.LogMessage;
                case "Log Minimal": return TraceCommandTypes.LogMessageMini;
                case "Log Verbose": return TraceCommandTypes.LogMessageVerb;
                case "Chain Message": return TraceCommandTypes.MoreInfo;
                case "Trace Message": return TraceCommandTypes.TraceMessage;
                case "Trace Enter": return TraceCommandTypes.TraceMessageIn;
                case "Trace Exit": return TraceCommandTypes.TraceMessageOut;
                case "Warning Message": return TraceCommandTypes.WarningMsg;
                case "Section Start": return TraceCommandTypes.SectionStart;
                case "Section End": return TraceCommandTypes.SectionEnd;
                case "Resource Allocation": return TraceCommandTypes.ResourceEat;
                case "Resource DeAllocation": return TraceCommandTypes.ResourcePuke;
                case "Resource Value Setting": return TraceCommandTypes.ResourceCount;
                case "XML Command": return TraceCommandTypes.CommandXML;
                case "Custom Command": return TraceCommandTypes.Custom;
                default: throw new ArgumentException("Invalid String passed to ReadableStringToTraceCommand", "tctstring invalid: " + tcstring);
            }
        }
    }

    /// <summary>
    /// Class holding all of the constants that are used by the trace program.
    /// </summary>
    public static class Constants {
        public const string THREADINITIDENTIFIER = ">TRCTNM<";

        public const int COMMANDSTRINGLENGTH = 5;

        // Remove this false positive.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "False positive on concatenated constatns")]
        public const string DATAINDICATOR = "~~DATA::"; // Some special more info commands send data to the viewer rather than debug messages

        /// <summary>
        /// When two messages are passed into a trace string they are separated like this for sending.
        /// </summary>
        public const string SECONDARYSTRINGSEPARATOR = "~~#~~";

        public const string MSGFMT_LOG = "#LOG#";
        public const string MSGFMT_LOGVERBOSE = "#LGV#";
        public const string LOGMESSAGEMINI = "#LGM#";
        public const string INTERNALMSG = "#INT#";

        public const string TRACEMESSAGEIN = "#TRI#";
        public const string TRACEMESSAGEOUT = "#TRO#";
        //public const string TRACEMESSAGE    = "#TRC#";

        public const string ASSERTIONFAILED = "#AST#";

        public const string MOREINFO = "#MOR#";
        public const string COMMANDONLY = "#CMD#";
        public const string MSGFMT_XMLCOMMAND = "#XCM#";
        public const string MSGFMT_CUSTOM = "#CUS#";
        public const string ERRORMSG = "#ERR#";
        public const string WARNINGMSG = "#WRN#";
        public const string EXCEPTIONBLOCK = "#EXC#";
        public const string EXCEPTIONDATA = "#EXD#";
        public const string EXCSTART = "#EXS#";
        public const string EXCEND = "#EXE#";

        public const string SECTIONSTART = "#SEC#";  // Sections are used for timings now too.
        public const string SECTIONEND = "#SXX#";

        public const string RESOURCEEAT = "#REA#";
        public const string RESOURCEPUKE = "#RPU#";
        public const string RESOURCECOUNT = "#RSC#";
        public const string TIMERNAME = "TexTimer";

        public const string TIMER_SECTIONIDENTIFIER = "|TMRCHK|";  // Timers use sections for sending to Mex therefore this identifies a timer rather than a normal section
        public const string TIMER_STRINGENDDELIMITER = "#X_X#]";
        public const string TIMER_STRINGSTARTDELIMITER = "[#X_X#";
        public const string TIMER_STRINGSTARTCONTENTSTRING = "TMRDATA" + TIMER_STRINGSTARTDELIMITER;
        public const string AUTOTIMER_PREFIX = "TAT_PFX";

        public const string TCPEND_MARKERTAG = "#TCPLIST-END#";
        public const int TCPEND_MARKERTAGLEN = 13;    // TCPEND_MARKERTAG.Length doesent work.  Used to work in uber.

        public const int DEFAULTPORT_NUMBER = 9060;
        public const string DEFAULTIP_ADDRESS = "*";

        // These are used by trace enter and exits to signify that some public replacements need to be done.
        public const string INTERNAL_CLASSMETHOD_REPLACE = "[X_TXCLS_::_TXMTH_X]";

        public const string INTERNAL_CLASS_REPLACE = "[X_TXCLS_X]";
        public const string INTERNAL_METHOD_REPLACE = "[X_TXCLS_::_TXMTH_X]";
        public const string INTERNAL_PARAMS_REPLACE = "[X_PRMS_X]";
        public const string INTERNAL_METHODTAGINDICATOR = "[X_";

        public const string NOT_IMP_MESSAGE = "NotImplemented:  An area of code that has not yet been implemented has been executed.";
        public const string URC_CODE_MESSAGE = "Unreachable Code Assertion Failure.  This code should not logically have been executed.";
        public const string URC_CODECTXT_MESSAGE = "A code path that was never expected to be possible has been reached.  The developer has not expected this eventuality and therefore the results of continuing are unpredicatable.";

        /// <summary>
        /// Loops that are based on recursion have depth protection enabled so that an assign innter to outer style
        /// nastyness does not break.  The output will be very ugly but it will be better to a stackoverflow
        /// excepiton, or nasty infinite looping thing.
        /// </summary>
        public const int DEPTHPROTECTION = 255;

        /// <summary>
        /// there is a limit to display text, this is most prevalant in evidence names which are built up from type names
        /// and therefore can easily exceed the length that they are sposed to be at.  This just visciously truncates them
        /// at that length.
        /// </summary>
        public const int EVIDENCENAMELENGTH = 50;

        /// <summary>
        /// public identifier to mark the end of an exception set of log messages
        /// </summary>
        public const string EXCEPTIONENDTAG = "X#EXCEPTIONEND#X";

        /// <summary>
        /// Command string to increase the indent of the viewer that is attached
        /// </summary>
        public const string CMD_INCREASEINDENT = "%INC%";

        /// <summary>
        /// Command string to decrease the indent of the viewer that is attached
        /// </summary>
        public const string CMD_DECREASEINDENT = "%DEC%";

        /// <summary>
        /// Command string to indicate to the viewer that no line should be left between this statement and the next
        /// </summary>
        public const string CMD_DONTLEAVELINE = "%NOC%";

        /// <summary>
        /// Command string for the viewer to indicate that a purge of the current process should be performed
        /// </summary>
        public const string CMD_CAUSEPURGEINCURRENT = "%PUC%";

        /// <summary>
        /// Command string for the viewer to indicate that a global purge should be performed
        /// </summary>
        public const string CMD_CAUSEGLOBALPURGE = "%PUG%";

        /// <summary>
        /// The truncate data end marker identifies where the end of the data that is associated with the truncation happens and therefore
        /// where the start of the actual truncated data begins.  Immediately following the last # the data begins.
        /// </summary>
        public const string TRUNCATE_DATAENDMARKER = "]#E#";

        public const string GENERIC_DIVIDER_MARKER = "#^#";

        /// <summary>
        /// The message truncate marker is placed at the start of truncation messages and at the end of messages that expect another message
        /// to follow them containing truncate data.  This had to be more uniqued up as we search for it now that it is no longer able to be
        /// used just as a start and the end of a string.  This is mostly used for pulling out the unqiue identifier that is used to join strings back
        /// together in the viewer.
        /// </summary>
        public const string MESSAGETRUNCATE = "#X~>TNKX<~X#"; // Had to increase the uniqueness of this marker.

        /// <summary>
        /// The name of the specific Output debug string listener
        /// </summary>
        public const string ODSLISTNER_NAME = "Trc_OdsListener";



        /// <summary>
        ///  The name of the external text writer listener
        /// </summary>
        public const string FILELISTNER_NAME = "TextWriterTraceListener"; // TODO : check this is the correct VS2003 / 5 textwriterlistener name


        // These must be used in a switch statment therefore cant be readonly statics

        /// <summary>
        /// The name for the default listener
        /// </summary>
        public const string DEFLISTNER_NAME = "TrcDefaultListener";

        /// <summary>
        /// The name for the VS.Net 2003 default listener
        /// </summary>
        public const string ORIGINALLISTENER_NAME = "Default";

        /// <summary>
        /// The internal string used when no further infromation is available.
        /// </summary>
        internal const string NFI = "No further information";

        // Restriction on the length of messages that are sent as one hit.  This limit will cause the
        // messages to be split into multiple chunks and then sent. This appears to be a limit of the
        // OutputDebugString win32 api function of 4096 bytes
        public const int LIMIT_OUTPUT_DATA_TO = 4050;

        // Each TraceInternal message type has an identifier associated with it so that the viewer can
        // process it differently if required.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Looks like a false positive, tpc_constructandinit - initialise method line 157")]
        internal const string TIMF_INITIALISECALLEDAGAIN = "**001**";  // Initialise called 2x for same process. Common in asp.

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Looks like a false positive, tpc_constructandinit - initialise method line 157")]
        internal const string TIMF_INITIALISECALLEDONCE = "**002**";  // Initialise called for the first time

        // When an assertion occurs the full assertion is sent to the trace stream as a single assertion message and some accompanying data
        // the accompanying data looks like assertion data but has this tag on the front of the message.
        internal const string ASSERTIONRECREATESTRING = "ASSERTDATA|";

        // The resource commands send name value pairs through in the debug message to indicate consumption and releases of
        // resources.  They can also send constant resource values through for the two different types of resource.
        // these strings are used as markers within the debug message to hold this information therefore are used by the consumer and
        // the viewer.
        internal const string RESPACKSTRINGIDENT = "RESIDENTSTRING|";

        internal const string RESNAMEDELIMITER = "/RNAME/";
        internal const string RESVALUEDELIMITER = "/RVALUE/";
        internal const string RESCONTEXTDELIMITER = "/RCTXT/";
        internal const string FORMATTEDRESOURCESTRING = RESPACKSTRINGIDENT + RESNAMEDELIMITER + "{0}" + RESNAMEDELIMITER + RESVALUEDELIMITER + "{1}" + RESVALUEDELIMITER + RESCONTEXTDELIMITER + "{2}" + RESCONTEXTDELIMITER;

        //internal const string RESNAMEDELIMPAIR = RESNAMEDELIMITER + RESVALUEDELIMITER;
        internal const int RESNAMEDELIMITERLENGTH = 7;
    }
}