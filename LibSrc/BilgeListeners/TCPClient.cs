#if false
namespace BilgeListeners {
    public class TCPClient {
        // If debugging then with stop on throw then this pops up all the time if its failing to connect, have therefore
        // dropped it right down such that it only retries once every 5 minutes.
        private const int SECONDS_NO_SOCKET_RETRY = 60;

        private string m_ipAddress;
        private int m_port;
        private TcpClient m_tcpClient;
        private NetworkStream m_stream;
        private Thread m_dispatcherThread; // thread on which output will be dispatched
        private Queue<string> m_queuedMessages; // queue to hold trace output waiting to be dispatched
        private object m_lockqueue;
        private ManualResetEvent m_mrse;

        /// <summary>
        /// Initialise the listener setting and start the dispatcher thread
        /// </summary>
        public void initialize(string ipAddress, int port) {
            m_ipAddress = ipAddress;
            m_port = port;

            m_lockqueue = new object();
            m_mrse = new ManualResetEvent(false);
            m_queuedMessages = new Queue<string>();
            ThreadStart ts = new ThreadStart(DispatcherThreadMethod);

            m_dispatcherThread = new Thread(ts);
            m_dispatcherThread.IsBackground = true;
            m_dispatcherThread.Start();
        }

        /// <summary>
        /// Add output string to queue for processing
        /// </summary>
        /// <param name="output">Trace output string</param>
        public void queueMessage(string output) {
            if (m_lockqueue == null) {
                // If the TCPListener is removed from the listeners collection then it will terminate its dispatcher thread.  This will place
                // the listener in an invalid state, if somehow a new message comes through when it is in this state then this exception
                // is thrown to alert the calling application.
                throw new InvalidOperationException("TCPListener internal TCP Dispatcher thread has been stopped, probably due to this listener having been removed from the collection of listeners.");
            }

            lock (m_lockqueue) {
                m_queuedMessages.Enqueue(output);
                m_mrse.Set();
            }
        }

        /// <summary>
        /// The TexTCP listeners main dispatcher thread which handles the sending of messages from the Tex component to the TCP stream.
        /// </summary>
        private void DispatcherThreadMethod() {
            Thread.CurrentThread.Name = "TcpListenerDispatcher";
            Thread.CurrentThread.IsBackground = true;
            StringBuilder sb = new StringBuilder();

            try {
                // Put in a wait timeout to ensure that when Tex is destroyed the background thread succesfully terminates. Worst case scenario here
                // is that it hangs around for quarter of a second
                while (true) {
                    if (!m_mrse.WaitOne(250, false) && (m_queuedMessages.Count == 0)) {
                        continue;
                    }

                    sb.Length = 0;
                    lock (m_lockqueue) {
                        while (m_queuedMessages.Count > 0) {
                            sb.Append(m_queuedMessages.Dequeue() + Constants.TCPEND_MARKERTAG);
                        }
                    }
                    if (sb.Length > 0) {
                        dispatchOutput(sb.ToString());
                    }

                    if (!m_mrse.Reset()) {
                        // The Win32Exception constructor provides an error number and message if you dont specify one
                        throw new Win32Exception();
                    }
                }
            } catch (Exception ex) {
                InternalUtil.LogInternalError(InternalUtil.InternalErrorCodes.TCPListenerError, "Exception in TCPListener Thread:" + ex.Message);
                throw;
            }
            // Destroy resources associated with thread  NEVER GETS HERE LOOP IS INFINITE
            //m_queuedMessages = null;
            //m_mrse = null;
            //m_lockqueue = null;
        }

        private DateTime lastSocketException = DateTime.MinValue;
        private bool socketCommunicationsDown;

        /// <summary>
        /// Method responsible for actual writing of output to tcp stream
        /// </summary>
        /// <param name="output">Output string</param>
        private void dispatchOutput(string output) {
            // if <60 and errored then waitone return
            if ((socketCommunicationsDown) && (DateTime.Now - lastSocketException).TotalSeconds < SECONDS_NO_SOCKET_RETRY) {
                // Ensure that we only retry the socket connection once a minute.
                m_mrse.Reset();
                return;
            }
            socketCommunicationsDown = false;

            Byte[] data = Encoding.ASCII.GetBytes(output);

            if ((m_tcpClient == null) || (!m_tcpClient.Connected)) {
                try {
                    m_tcpClient = new TcpClient(m_ipAddress, m_port);
                    m_stream = m_tcpClient.GetStream();
                } catch (SocketException sx) {
                    lastSocketException = DateTime.Now;
                    socketCommunicationsDown = true;
                    InternalUtil.LogInternalError(InternalUtil.InternalErrorCodes.TCPListenerError, "Socket Exception : " + sx.Message, TraceLevel.Warning);
                    m_mrse.Reset();
                    return;
                }
            }

            // Should now have reconnected or at least tried to connect.
            try {
                m_stream.Write(data, 0, data.Length);
                m_stream.Flush();
            } catch (IOException iox) {
                InternalUtil.LogInternalError(InternalUtil.InternalErrorCodes.TCPListenerError, "Socket IO Exception : " + iox.Message, TraceLevel.Warning);
                m_mrse.Reset();
                return;
            }
        }
    }
}
#endif