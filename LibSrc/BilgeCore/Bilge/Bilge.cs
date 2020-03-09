
namespace Plisky.Diagnostics {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;

    public class Bilge {

        private static Func<string, SourceLevels, SourceLevels> levelResolver = DefaultLevelResolver;

        private static SourceLevels DefaultLevelResolver(string source, SourceLevels beforeModification) {

            SourceLevels result = beforeModification;
#if NETSTD2
            
#else

#endif

            return result;
        }


        public static void SetConfigurationResolver(Func<string, SourceLevels, SourceLevels> lr){
            levelResolver = lr;
        }


        private ConfigSettings activeConfig;


        /// <summary>
        /// gets or Sets the current level for tracing - this will use the TraceLevel enum to determine which of the logging functions
        /// will write data out.  The order of increasing data is off, error, warning, info, verbose.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the trace level is set outside of the defined ranges.</exception>
        public TraceLevel CurrentTraceLevel {
            get { return activeConfig.GetLegacyTraceLevel(); }
            set {
                SetTraceLevel(value);
            }
        }

        public SourceLevels ActiveTraceLevel {
            get { return activeConfig.activeTraceLevel; }
            set { SetTraceLevel(value); }
        }

        public bool IsCleanInitialise() {
            if (!BilgeRouter.Router.IsClean()) {
                return false;
            }
            return true;
        }

        private void SetTraceLevel(SourceLevels value) {
            if (activeConfig.activeTraceLevel == value) { return; }
            if (!Enum.IsDefined(typeof(SourceLevels), value)) {
                throw new ArgumentException($"The value passed to {nameof(CurrentTraceLevel)} property must be one of the TraceLevel enum values", "value");
            }

            activeConfig.activeTraceLevel = value;
            this.Error.IsWriting = (activeConfig.activeTraceLevel & SourceLevels.Error) == SourceLevels.Error;
            this.Warning.IsWriting = (activeConfig.activeTraceLevel & SourceLevels.Warning) == SourceLevels.Warning;
            this.Info.IsWriting = (activeConfig.activeTraceLevel & SourceLevels.Information) == SourceLevels.Information;
            this.Verbose.IsWriting = (activeConfig.activeTraceLevel & SourceLevels.Verbose) == SourceLevels.Verbose;
        }
        private void SetTraceLevel(TraceLevel value) {
            SetTraceLevel(Bilge.ConvertTraceLevel(value));            
        }

        public static SourceLevels ConvertTraceLevel(TraceLevel value) {

            SourceLevels result = SourceLevels.Off;

            switch (value) {
                case TraceLevel.Off: result = SourceLevels.Off; break;
                case TraceLevel.Error: result = SourceLevels.Error | SourceLevels.Critical; break;
                case TraceLevel.Warning: result = SourceLevels.Error | SourceLevels.Critical | SourceLevels.Warning; break;
                case TraceLevel.Info: result = SourceLevels.Error | SourceLevels.Critical | SourceLevels.Warning | SourceLevels.Information; break;
                case TraceLevel.Verbose: result = SourceLevels.Error | SourceLevels.Critical | SourceLevels.Warning | SourceLevels.Information | SourceLevels.Verbose | SourceLevels.Information; break;
            }

            return result;
        }


        /// <summary>
        /// Bilge provides developer level trace to provide runtime diagnostics to developers.  
        /// </summary>
        /// <param name="selectedInstanceContext">The context for this particular instance of bilge, usually used to identify a subsystem</param>
        /// <param name="sessionContext">The context for a session, usually used to identify the user request</param>
        /// <param name="tl">The trace level to set this instance of bilge to</param>
        /// <param name="resetDefaults">Reset all pf the internal context of Bilge</param>
        public Bilge(string selectedInstanceContext = "-", string sessionContext = "-", SourceLevels tl = SourceLevels.Off, bool resetDefaults = false) {
            activeConfig = new ConfigSettings();
            activeConfig.InstanceContext = selectedInstanceContext;
            activeConfig.SessionContext = sessionContext;
            string procId = Process.GetCurrentProcess().Id.ToString();

            if (resetDefaults) {
                BilgeRouter.Router.ClearEverything();
            }

            Assert = new BilgeAssert(BilgeRouter.Router, activeConfig);
            Info = new BilgeWriter(BilgeRouter.Router, activeConfig, SourceLevels.Information | SourceLevels.Error | SourceLevels.Critical );
            Verbose = new BilgeWriter(BilgeRouter.Router, activeConfig, SourceLevels.All);
            Warning = new BilgeWriter(BilgeRouter.Router, activeConfig, SourceLevels.Warning | SourceLevels.Error | SourceLevels.Critical);
            Error = new BilgeWriter(BilgeRouter.Router, activeConfig, SourceLevels.Error | SourceLevels.Critical );
            Critical = new BilgeWriter(BilgeRouter.Router, activeConfig, SourceLevels.Critical);
            Direct = new BilgeDirect(BilgeRouter.Router, activeConfig);
            Util = new BilgeUtil(BilgeRouter.Router, activeConfig);
            
            var level = levelResolver(selectedInstanceContext, tl);
            SetTraceLevel(level);
        }

        public BilgeUtil Util { get; private set; }

        /// <summary>
        /// Informational logging, designed for program flow and basic debugging.  Provides a good detailed level of logging without going into
        /// immense details.
        /// </summary>
        public BilgeWriter Info { get; private set; }

        /// <summary>
        /// The fullest level of most detailed logging, includes additional data and secondary messages to really help get detailed information on
        /// the execution of the code. This is the most detailed and therefore slowest level of logging.
        /// </summary>
        public BilgeWriter Verbose { get; private set; }

        /// <summary>
        /// Allows warning level logging, used for concerning elements of the code that do not necesarily result in errors.  
        /// </summary>
        public BilgeWriter Warning { get; private set; }

        /// <summary>
        /// Errors that are recoverable from, indicates non fatal problems in the code execution paths.
        /// </summary>
        public BilgeWriter Error { get; private set; }

        /// <summary>
        /// Fatal log events, the program is in a bad state and about to terminate or should be terminated.  The critical logging levels are designed to give
        /// an overview of why a program failed.
        /// </summary>
        public BilgeWriter Critical { get; private set; }

        public BilgeAssert Assert { get; private set; }

        /// <summary>
        /// Provides direct writing to the output debug stream using your own values for methods.  This always writes, irrespective of the trace level
        /// but it is sitll subject to settings such as write on fail and queueing.
        /// </summary>
        public BilgeDirect Direct { get; private set; }

        public bool WriteOnFail {
            get {
                return BilgeRouter.Router.WriteToHandlerOnlyOnFail;
            }
            set {
                if (value != BilgeRouter.Router.WriteToHandlerOnlyOnFail) {
                    BilgeRouter.Router.WriteToHandlerOnlyOnFail = value;
                }
            }
        }

        public void SetMessageBatching(int numberMessagesInABatch = 100, int millisecondsToWaitForBatch = 100) {
            BilgeRouter.Router.MessageBatchCapacity = numberMessagesInABatch;
            BilgeRouter.Router.MessageBatchDelay = millisecondsToWaitForBatch;
        }

        public void DisableMessageBatching() {
            BilgeRouter.Router.MessageBatchDelay = 0;
            BilgeRouter.Router.MessageBatchCapacity = 0;
        }

        public void AddHandler(IBilgeMessageHandler ibmh) {
            BilgeRouter.Router.AddHandler(ibmh);
        }

        public void TriggerWrite() {
            BilgeRouter.Router.FailureOccuredForWrite = true;
        }

        /// <summary>
        /// This method returns a string describing the current internal logging status of Bilge.  If there is no output going to your chosen
        /// listener then this method can help track down what is wrong.
        /// </summary>
        /// <returns></returns>
        public string GetDiagnosticStatus() {
            StringBuilder result = new StringBuilder();
            result.Append($" {nameof(Info)} Writing: {Info.IsWriting} \n");
            result.Append($" {nameof(Verbose)} Writing: {Verbose.IsWriting} \n");
            result.Append($" {nameof(Warning)} Writing: {Warning.IsWriting} \n");
            result.Append($" {nameof(Error)} Writing: {Error.IsWriting} \n");

            result.Append(BilgeRouter.Router.GetHandlerStatuses());
            return result.ToString();
        }


        public const string BILGE_INSTANCE_CONTEXT_STR = "bc-ctxt";
        public const string BILGE_SESSION_CONTEXT_STR = "bc-sess-ctxt";

        /// <summary>
        /// Adds a context name value pair to every singe message that is routed through this instance of bilge.
        /// </summary>
        /// <param name="context">Context Name - e.g. userSession</param>
        /// <param name="value">Context Value - e.g. 123456</param>
        public void AddContext(string context, string value) {

        }
        /// <summary>
        /// This method shuts down bilge but it is the only way to be sure that all of your messages have left the
        /// internal queuing system.  Once this method has run Bilge is completely broken and no more trace can be
        /// written, therefore it should only be used when a process is exiting and you still want to keep all off
        /// the trace messages - for example during a console program.
        /// </summary>
        /// <remarks>It is possible to reinitialise with reinit=true, but this is not recommended.</remarks>
        public void FlushAndShutdown(bool reinit = false) {
            BilgeRouter.Router.Shutdown();
            if (reinit) {
                BilgeRouter.Router.ReInitialise();
            }
        }

        public void Flush() {

            BilgeRouter.Router.FlushMessages();


        }

        public static void CollapseRouter() {
            BilgeRouter.Router.Shutdown();
        }

        public static void AddMessageHandler(IBilgeMessageHandler ibmh) {
            BilgeRouter.Router.AddHandler(ibmh);
        }
    }
}

