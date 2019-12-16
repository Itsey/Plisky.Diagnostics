
namespace Plisky.Diagnostics {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    public class Bilge {
        protected Dictionary<string, string> metaContexts { get; set; }
        private ConfigSettings activeConfig;


        /// <summary>
        /// gets or Sets the current level for tracing - this will use the TraceLevel enum to determine which of the logging functions
        /// will write data out.  The order of increasing data is off, error, warning, info, verbose.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the trace level is set outside of the defined ranges.</exception>
        public TraceLevel CurrentTraceLevel {
            get { return activeConfig.activeTraceLevel; }
            set {
                SetTraceLevel(value);
            }
        }

        public bool IsCleanInitialise() {
            if (!BilgeRouter.Router.IsClean()) {
                return false;
            }
            return true;
        }

        private void SetTraceLevel(TraceLevel value) {
            if (activeConfig.activeTraceLevel == value) { return; }
            if (!Enum.IsDefined(typeof(TraceLevel), value)) {
                throw new ArgumentException($"The value passed to {nameof(CurrentTraceLevel)} property must be one of the TraceLevel enum values", "value");
            }
            activeConfig.activeTraceLevel = value;
            this.Error.IsWriting = (activeConfig.activeTraceLevel >= TraceLevel.Error);
            this.Warning.IsWriting = (activeConfig.activeTraceLevel >= TraceLevel.Warning);
            this.Info.IsWriting = (activeConfig.activeTraceLevel >= TraceLevel.Info);
            this.Verbose.IsWriting = (activeConfig.activeTraceLevel >= TraceLevel.Verbose);
        }

        /// <summary>
        /// Bilge provides developer level trace to provide runtime diagnostics to developers.  
        /// </summary>
        /// <param name="selectedInstanceContext">The context for this particular instance of bilge, usually used to identify a subsystem</param>
        /// <param name="sessionContext">The context for a session, usually used to identify the user request</param>
        /// <param name="tl">The trace level to set this instance of bilge to</param>
        /// <param name="resetDefaults">Reset all pf the internal context of Bilge</param>
        public Bilge(string selectedInstanceContext = "-",string sessionContext = "-", TraceLevel tl = TraceLevel.Info, bool resetDefaults = false) {
            activeConfig = new ConfigSettings();
            activeConfig.InstanceContext = selectedInstanceContext;
            activeConfig.SessionContext = sessionContext;
            string procId = Process.GetCurrentProcess().Id.ToString();

            if (resetDefaults) {
                BilgeRouter.Router.ClearEverything();
            }

            Assert = new BilgeAssert(BilgeRouter.Router, activeConfig, selectedInstanceContext);
            Info = new BilgeWriter(BilgeRouter.Router, activeConfig, TraceLevel.Info);
            Verbose = new BilgeWriter(BilgeRouter.Router, activeConfig, TraceLevel.Verbose);
            Warning = new BilgeWriter(BilgeRouter.Router, activeConfig, TraceLevel.Warning);
            Error = new BilgeWriter(BilgeRouter.Router, activeConfig, TraceLevel.Error);
            SetTraceLevel(tl);
        }

        public BilgeWriter Info { get; private set; }
        public BilgeWriter Verbose { get; private set; }

        public BilgeWriter Warning { get; private set; }
        public BilgeWriter Error { get; private set; }

        public BilgeAssert Assert { get; private set; }

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

        /// <summary>
        /// Adds a context name value pair to every singe message that is routed through this instance of bilge.
        /// </summary>
        /// <param name="context">Context Name - e.g. userSession</param>
        /// <param name="value">Context Value - e.g. 123456</param>
        public void  AddContext(string context, string value) {

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
    }
}

