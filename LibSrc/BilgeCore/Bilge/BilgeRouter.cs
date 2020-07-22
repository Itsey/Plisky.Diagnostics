

namespace Plisky.Diagnostics {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    public abstract class BilgeRouter {
        public static BilgeRouter Router = new QueuedBilgeRouter(Process.GetCurrentProcess().Id.ToString(), "Unknown");


        protected string PerformSupportedReplacements(MessageMetadata mmd, string msg) {
            msg = msg.Replace("%TS%", String.Format("{0,0:D2}:{1,0:D2}:{2,0:D2}.{3,0:D3}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond));

            msg = msg.Replace("%MN%", mmd.MethodName);

            return msg;
        }

        public BilgeRouter(string processId, string machineName) {
            this.ProcessIdCache = processId;
            this.MachineNameCache = machineName;
            string mn = "Unknown";
            try {
                mn = Environment.MachineName;
            } catch (InvalidOperationException) {
                // Swallow this - on containers cant get machine name.
            }
            this.MachineNameCache = mn;

        }

        public string MachineNameCache { get; private set; }


        protected bool HaveHandlers = false;
        protected string ProcessIdCache { get; private set; }


        protected int lastUsedHandler = -1;
        protected IBilgeMessageHandler[] handlers;

        public bool QueueMessages { get; set; }
        public bool FailureOccuredForWrite { get; set; }


        protected abstract void ActualAddMessage(MessageMetadata mmd);
        internal abstract void ActualClearEverything();
        protected abstract bool ActualIsClean();
        protected abstract void ActualAddHandler(IBilgeMessageHandler ibmh);
        protected abstract string ActualGetHandlerStatuses();
        internal abstract void ActualReInitialise();
        internal abstract void ActualShutdown();

        internal void PrepareMetaData(MessageMetadata mmd, Dictionary<string,string> contextKeys) {
           
            mmd.NetThreadId = Thread.CurrentThread.ManagedThreadId.ToString(); 
            mmd.OSThreadId = Thread.CurrentThread.ManagedThreadId.ToString(); // TODO ! HArdcoded
            mmd.Context = contextKeys[Bilge.BILGE_INSTANCE_CONTEXT_STR];
        }

        public void ReInitialise() {
            ActualReInitialise();
        }

        public void Shutdown() {
            ActualShutdown();
        }

        internal string GetHandlerStatuses() {
            return ActualGetHandlerStatuses();
        }

        internal IEnumerable<IBilgeMessageHandler> GetHandlers() {
            if (handlers==null) { yield break; }

            foreach(var f in handlers) {
                yield return f;
            }
        }

        internal void QueueMessage(MessageMetadata mm) {
            ActualAddMessage(mm);
        }

        protected abstract void ActualFlushMessages();

        internal void FlushMessages() {
            ActualFlushMessages();
        }
        internal void AddHandler(IBilgeMessageHandler ibmh) {
            ActualAddHandler(ibmh);
        }

        public bool WriteToHandlerOnlyOnFail { get; set; }

        public int MessageBatchCapacity { get; set; }
        public int MessageBatchDelay { get; set; }

        internal bool IsClean() {
            return ActualIsClean();
        }


        public void ClearEverything() {
            Shutdown();
            ActualClearEverything();
            handlers = null;
            ReInitialise();
        }
    }



  
}

