
namespace Plisky.Diagnostics {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;

    public class Bilge {

        // OBSOLETE >>> REMOVE NEXT MAJOR VER

        /// <summary>
        /// BACKCOMPAT - Legacy Method For Compatibility - Gets or Sets the current level for tracing - this will use the TraceLevel enum to determine which of the logging functions
        /// will write data out.  The order of increasing data is off, error, warning, info, verbose.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the trace level is set outside of the defined ranges.</exception>
        [Obsolete("Replaced by ActiveTraceLevel, switching to using SourceLevels rather than TraceLevels")]
        public TraceLevel CurrentTraceLevel {
            get { return activeConfig.GetLegacyTraceLevel(); }
            set {
                SetTraceLevel(value);
            }
        }


        // OBSOLETE ^^^^^

        // Private
        private ConfigSettings activeConfig;
        private static Func<string, SourceLevels, SourceLevels> levelResolver = DefaultLevelResolver;
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

        private static SourceLevels DefaultLevelResolver(string source, SourceLevels beforeModification) {

            SourceLevels result = beforeModification;
#if NETSTD2
            
#else

#endif

            return result;
        }

        // Public
        public const string BILGE_INSTANCE_CONTEXT_STR = "bc-ctxt";
        public const string BILGE_SESSION_CONTEXT_STR = "bc-sess-ctxt";

        public IEnumerable<Tuple<string,string>> GetContexts() {
            foreach(var l in activeConfig.metaContexts.Keys) {
                yield return new Tuple<string, string>(l, activeConfig.metaContexts[l]);
            }
        }

   

        /// <summary>
        /// Establishes the active trace level, using a SourceLevel.  Passing a source leve to this sets the trace level for this instance of Bilge.
        /// </summary>
        public SourceLevels ActiveTraceLevel {
            get { return activeConfig.activeTraceLevel; }
            set { SetTraceLevel(value); }
        }

#if DEBUG

        /// <summary>
        /// Used for unit esting only, are we in an initialised state>?
        /// </summary>
        /// <returns>True if bilge freshly initialised</returns>
        public bool IsCleanInitialise() {
            if (!BilgeRouter.Router.IsClean()) {
                return false;
            }
            return true;
        }

#endif

        /// <summary>
        /// Sets up a configuraiton resolver that is called for every new instance of Bilge.  This will be called with the instance name
        /// and the current trace level of the instance.  The return is your new desireds trace level.  This can be used to turn on logging
        /// based on configuration or any other external factor with minimal impact on your code base.
        /// </summary>
        /// <param name="configurationResolver"></param>
        public static void SetConfigurationResolver(Func<string, SourceLevels, SourceLevels> configurationResolver) {
            levelResolver = configurationResolver;
        }

        /// <summary>
        /// Removes the static configuration resolver to ensure that resolution returns to the default.  Note any instances that have been
        /// configured by the resolver will remain configured.  This clear will only affect new instances.
        /// </summary>
        public static void ClearConfigurationResolver() {
            levelResolver = DefaultLevelResolver;
        }

        /// <summary>
        /// Converts a TraceLevel into a SourceLevels, to allow you to continue to use old code that supports trace levels and work with the change to source
        /// levels within Bilge, used by the legacy support for CurrentTraceLevel.
        /// </summary>
        /// <param name="value">The TraceLevel to use</param>
        /// <returns>Opinionated conversion to SourceLevel.</returns>
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
            Info = new BilgeWriter(BilgeRouter.Router, activeConfig, SourceLevels.Information | SourceLevels.Error | SourceLevels.Critical);
            Verbose = new BilgeWriter(BilgeRouter.Router, activeConfig, SourceLevels.All);
            Warning = new BilgeWriter(BilgeRouter.Router, activeConfig, SourceLevels.Warning | SourceLevels.Error | SourceLevels.Critical);
            Error = new BilgeWriter(BilgeRouter.Router, activeConfig, SourceLevels.Error | SourceLevels.Critical);
            Critical = new BilgeWriter(BilgeRouter.Router, activeConfig, SourceLevels.Critical);
            Direct = new BilgeDirect(BilgeRouter.Router, activeConfig);
            Util = new BilgeUtil(BilgeRouter.Router, activeConfig);

            var level = levelResolver(selectedInstanceContext, tl);
            SetTraceLevel(level);
        }

        /// <summary>
        /// Provides alerting, specific methods for alerting, writes to the stream irrespective of the trace level.  Most slowdown elements are disabled
        /// and specific method types are provided for alerting, such as applicaiton online, offline etc.  
        /// </summary>
        public static BilgeAlert Alert { get; } = new BilgeAlert();

        /// <summary>
        /// Provies access to Bilge Utility functions and debugging and diagnostic helper methods.
        /// </summary>
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

        /// <summary>
        /// Provides assertion capabilities, runtime checks that should only really be performed during development builds or to validate that something which
        /// should be handled elsewhere in code has really been handled.
        /// </summary>
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
            Bilge.ForceFlush();            
        }


        // **** Static ****

        public static void ForceFlush() {

            BilgeRouter.Router.FlushMessages();


        }

        public static void CollapseRouter() {
            BilgeRouter.Router.Shutdown();
        }

        [Obsolete("Replaced by AddHandler,takes Options.  Migrate your code to AddHandler(hnd, HandlerOptions.Duplicates) for compat.")]
        public static void AddMessageHandler(IBilgeMessageHandler ibmh) {
            BilgeRouter.Router.AddHandler(ibmh);
        }

        /// <summary>
        /// Adds a Handler based on the handler options - handler options can either check for matching types, or matching names, and can 
        /// refuse to add if a matching type or name is added.  Default is to allow as many duplicate handlers as you wish.
        /// </summary>
        /// <param name="ibmh">The handler to add</param>
        /// <param name="hao">The approach to take with duplicates</param>
        /// <returns></returns>
        public static bool AddHandler(IBilgeMessageHandler ibmh, HandlerAddOptions hao = HandlerAddOptions.Duplicates) {
            switch (hao) {
                
                case HandlerAddOptions.SingleType:
                    foreach (var n in GetHandlers()) {
                        if (n.GetType() == ibmh.GetType()) {
                            return false;
                        }
                    };
                    break;
                case HandlerAddOptions.SingleName:
                    var mn = ibmh.Name.ToLower();
                    foreach (var n in GetHandlers()) {
                        if (n.Name.ToLower() == mn) {
                            return false;
                        }
                    }
                    break;
                default:
                    break;
            }


            BilgeRouter.Router.AddHandler(ibmh);
            return true;
        }

        public static void ClearMessageHandlers() {
            BilgeRouter.Router.FlushMessages();
            BilgeRouter.Router.ClearEverything();
        }

        /// <summary>
        /// Gets all of the message handlers with some very basic filtering capability, this is not usually required by most implementations but can
        /// be useful when allowing for things like dynamic configuration of message handlers.  The filter string can either start or end with an  *
        /// to indicate that the name should be an exact match (no *) or end with (*text) or start with (text*) a matching text filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IEnumerable<IBilgeMessageHandler> GetHandlers(string filter = "*") {
            string nameMatchStart = null;
            string nameMatchEnd = null;

            if (filter.StartsWith("*")) {
                nameMatchEnd = filter.Substring(1);
            } else if (filter.EndsWith("*")) {
                nameMatchStart= filter.Substring(0, filter.Length - 1);
            }

            foreach (var nextHandler in BilgeRouter.Router.GetHandlers()) {

                if (!string.IsNullOrEmpty(nextHandler.Name)) {
                    if (!string.IsNullOrEmpty(nameMatchStart)) {
                        if (nextHandler.Name.StartsWith(nameMatchStart)) {
                            yield return nextHandler;
                        }
                    } else if (!string.IsNullOrEmpty(nameMatchEnd)) {
                        if (nextHandler.Name.EndsWith(nameMatchEnd)) {
                            yield return nextHandler;
                        }
                    } else {
                        yield return nextHandler;
                    }
                } else {
                    yield return nextHandler;
                }

                
            }

        }
    }
}

