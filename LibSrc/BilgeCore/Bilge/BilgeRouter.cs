

namespace Plisky.Diagnostics {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// BilgeRouter acts as a central point taking all of the inputs from the Bilge/BilgeWriter combinations and routing them through
    /// a static queue of messages that is handled by a dedicated thread.  Bilge2 no longer allows for non queued messages and therefore
    /// all messages will be queued here priort to being passed to the handlers.
    /// </summary>
    internal class BilgeRouter {
        public static readonly BilgeRouter Router = new BilgeRouter(Process.GetCurrentProcess().Id.ToString());
#if DEBUG
        private static int threadsActive = 0;
        private const int THREADWAITWHENNOEVENTS = 250;  // 10 seconds temporarily to allow for longer debugging time
#else
    private  const int THREADWAITWHENNOEVENTS = 5000;    // Normally 5 seconds would be ok
#endif

        internal bool IsClean() {
            if ((this.messageQueue != null) && (this.messageQueue.Count > 0)) {
#if ACTIVEDEBUG
                Emergency.Diags.Log($"CleanCheck {messageQueue.Count} messages.");
#endif
                return false;
            }
            if ((this.handlers != null) && (this.handlers.Length > 0)) {
#if ACTIVEDEBUG
                Emergency.Diags.Log($"CleanCheck {handlers.Length} handlers.");
#endif

                return false;

            }
            return true;
        }

        private Thread s_dispatcherThread;
        private ConcurrentQueue<MessageMetadata> messageQueue;
        private volatile int messageQueueMaximum = -1;
        private AutoResetEvent queuedMessageResetEvent;

        private volatile bool s_shutdownEnabled;

        private void EnableQueuedMessages() {
            if ((s_dispatcherThread != null) && (queuedMessageResetEvent != null)) {
                // Thread is running.
                if (s_shutdownEnabled) {
                    while (s_dispatcherThread != null) {
                        queuedMessageResetEvent.Set();
                        //not sure why this was here Thread.Sleep(0);
                    }
                } else {
                    // Its running and not shutting down just leave it.
                    return;
                }
            }

            s_shutdownEnabled = false;

            queuedMessageResetEvent = new AutoResetEvent(false);
            messageQueue = new ConcurrentQueue<MessageMetadata>();

#if DEBUG
            if (threadsActive == 1) {
                throw new InvalidOperationException("Attempting to bring online a second background thread");
            }
#endif
            s_dispatcherThread = new Thread(new ThreadStart(DispatcherThreadMethod));
            s_dispatcherThread.Start();
        }

        internal void ClearEverything() {
            Shutdown();
            WriteToHandlerOnlyOnFail = false;
            FailureOccuredForWrite = false;
            lastUsedHandler = -1;
            messageQueue = new ConcurrentQueue<MessageMetadata>();
            handlers = null;
            ReInitialise();
        }

        internal void QueueMessage(MessageMetadata mp) {
#if ACTIVEDEBUG
            Emergency.Diags?.Log($"Message queued " + mp.Body);
#endif

            if (shutdownRequested) {
                throw new NotSupportedException("ShutdownEnabled, this method should not be called when shutdown has begun");
            }

            if (!HaveHandlers) { return; }  // No handlers, just throw the content away.

            if (messageQueueMaximum > 0) {
                // Remove oldest messages until we come down below the limit again.
                while (messageQueue.Count >= messageQueueMaximum) {
                    while (!messageQueue.TryDequeue(out MessageMetadata mpx)) ;
                }
            }
            messageQueue.Enqueue(mp);

            queuedMessageResetEvent.Set();
        }

        private volatile bool shutdownRequested = false;

        internal void Shutdown() {
#if ACTIVEDEBUG
            Emergency.Diags?.Log($"Shutdown requested");
#endif

            if (queuedMessageResetEvent != null) {
                queuedMessageResetEvent.Set();
            }
            shutdownRequested = true;

            if (s_dispatcherThread != null) {
#if ACTIVEDEBUG
                Emergency.Diags?.Log($"Waiting on dispatcher thread");
#endif

                s_dispatcherThread.Join();
            }

            if (handlers != null) {
                for (int i = 0; i < handlers.Length; i++) {
                    var next = handlers[i] as IDisposable;
#if ACTIVEDEBUG
                    Emergency.Diags?.Log($"Disposing handler {i} {handlers[i].Name}");
#endif

                    if (next != null) {
                        next.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Called to write out the messages that are queued.
        /// </summary>
        private void TriggerQueueWrite() {
            queuedMessageResetEvent.Set();
        }

        internal void ReInitialise() {
            shutdownRequested = false;
            s_shutdownEnabled = false;
            EnableQueuedMessages();
        }

        private void DispatcherThreadMethod() {
#if DEBUG
            // As this is static and uses a lot of static data it should never be shared with another one of these threads. We put this check
            // into the development builds to verify that this does not occur.
            Interlocked.Increment(ref threadsActive);
            if (threadsActive > 1) {
                throw new InvalidOperationException("It should not be possible to have more than one thread active using the queues");
            }
            try {
#endif
                Thread.CurrentThread.Name = "Bilge>>RouterQueueDispatcher";
                Thread.CurrentThread.IsBackground = true;

                List<Task> activeTasks = new List<Task>();

                while ((!shutdownRequested) && (!System.Environment.HasShutdownStarted)) {
                    int i = 0;  // Clear down any tasks that have completed.
                    while (i < activeTasks.Count) {
                        if (activeTasks[i].IsCompleted) {
                            activeTasks.RemoveAt(i);
                        } else {
                            i++;
                        }
                    }

                    if (messageQueue.Count == 0) {
                        // If the thread has nothing to do it waits for the next message or 5 seconds in case theres a message that
                        // has come in since the zero was set or in case a shutdown request was made.
                        if (!queuedMessageResetEvent.WaitOne(THREADWAITWHENNOEVENTS, false)) {
                            // If we come out of our wait and there are still no events, or shutodwn is requested bomb out
                            if ((messageQueue.Count == 0) || (System.Environment.HasShutdownStarted)) {
                                continue;
                            }
                        }
                    }

                    if ((WriteToHandlerOnlyOnFail) && (!FailureOccuredForWrite)) {
                        // Not quite the same this, there are events but we dont want to deal with them
                        Thread.Sleep(THREADWAITWHENNOEVENTS);
                        continue;
                    }

                    // WriteOnFail means only write when failure occured-  at this point we reset.
                    if (FailureOccuredForWrite) { FailureOccuredForWrite = false; }

                    while ((messageQueue.Count > 0) && (!System.Environment.HasShutdownStarted)) {
                        MessageMetadata mp;
                        while (!messageQueue.TryDequeue(out mp)) ;
                        activeTasks.Add(RouteMessage(mp));
                    }
                }

                // Destroy resources associated with the high perf implementation
                messageQueue = null;
                queuedMessageResetEvent = null;

#if DEBUG
            } finally {
                Interlocked.Decrement(ref threadsActive);
            }
#endif
            // Remove the reference as the last thing we do
            s_dispatcherThread = null;
        }

        internal void FlushMessages() {
            int msgs = messageQueue.Count;


            Emergency.Diags.Log($"Flush, messages {msgs}");

            // This takes the current count of messages into msgs, and will run through processing
            // messages - until either there are none left

            while ((msgs > 0) && (messageQueue.Count > 0)) {
                queuedMessageResetEvent.Set();
                Thread.Sleep(10);
                msgs--;
            }

            if (handlers != null) {
                foreach (var h in handlers) {
                    h.Flush();
                }
            }

            Emergency.Diags.Log($"Flush, done ");

        }


#if NET452

        private async Task RouteMessage(MessageMetadata mp) {
            if ((handlers == null) || (handlers.Length == 0)) { return; }
            PrepareMessage(mp);

            Emergency.Diags.Log($"InternalRouteMessage >> " + mp.Body);

            var hndlr = handlers;

            Task[] tasks = new Task[hndlr.Length];
            for (int i = 0; i < hndlr.Length; i++) {
                Emergency.Diags.Log($"Handler {i} routing message");
                tasks[i] = hndlr[i].HandleMessageAsync(new MessageMetadata[] { mp });
            }
            await Task.WhenAll(tasks);


        }

#else
        private void dosomethig() {

        }

        private Task RouteMessage(MessageMetadata mp) {
            if ((handlers == null) || (handlers.Length == 0)) { return Task.Factory.StartNew(() => dosomethig()); }
            PrepareMessage(mp);

            var hndlr = handlers;
            Task[] tasks = new Task[hndlr.Length];
            for (int i = 0; i < hndlr.Length; i++) {
#if NET452 || NETSTANDARD2_0
                tasks[i] = hndlr[i].HandleMessageAsync(new MessageMetadata[] { mp });
#else
                hndlr[i].HandleMessage40(new MessageMetadata[] { mp });
#endif
            }

            return Task.Factory.StartNew(() => dosomethig());

        }

#endif


        int lastUsedHandler = -1;
        private IBilgeMessageHandler[] handlers;
        private string mname = null;

        public bool QueueMessages { get; set; }

        public bool FailureOccuredForWrite { get; set; }

        public string MachineNameCache {
            get {
                if (mname == null) {
                    mname = Environment.MachineName;
                }
                return mname;
            }
        }

        private bool HaveHandlers = false;
        public string ProcessIdCache { get; private set; }
        public bool WriteToHandlerOnlyOnFail { get; set; }

        internal void AddHandler(IBilgeMessageHandler ibmh) {
#if ACTIVEDEBUG
            Emergency.Diags?.Log($"AddingHandler {ibmh.Name}");
#endif

            IBilgeMessageHandler[] replacement;
            HaveHandlers = true;
            // Not sure on thread safety here - what happens if lastUsedHandler changes?
            // Dont want to wack a lock round the whole thing tho

            // TODO : Use Priority to order the handlers correctly.

            replacement = new IBilgeMessageHandler[lastUsedHandler + 2];
            for (int i = 0; i <= lastUsedHandler; i++) {
                replacement[i] = handlers[i];
            }

            lastUsedHandler++;
            replacement[lastUsedHandler] = ibmh;

            handlers = replacement;
        }

        private void PrepareMessage(MessageMetadata msg) {
            if (!string.IsNullOrEmpty(msg.Body)) {
                msg.Body = PerformSupportedReplacements(msg, msg.Body);
            }
            if (!string.IsNullOrEmpty(msg.FurtherDetails)) {
                msg.FurtherDetails = PerformSupportedReplacements(msg, msg.FurtherDetails);
            }
            msg.MachineName = this.MachineNameCache;
            msg.ProcessId = this.ProcessIdCache;
        }

        private string PerformSupportedReplacements(MessageMetadata mmd, string msg) {
            msg = msg.Replace("%TS%", String.Format("{0,0:D2}:{1,0:D2}:{2,0:D2}.{3,0:D3}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond));

            msg = msg.Replace("%MN%", mmd.MethodName);

            return msg;
        }

        private BilgeRouter(string processId) {
            this.ProcessIdCache = processId;

            EnableQueuedMessages();
        }

        internal void PrepareMetaData(MessageMetadata mmd, string ctxt) {
            mmd.NetThreadId = Thread.CurrentThread.ManagedThreadId.ToString();
            mmd.OSThreadId = "0"; // TODO ! HArdcoded
            mmd.Context = ctxt;
        }
    }
}

