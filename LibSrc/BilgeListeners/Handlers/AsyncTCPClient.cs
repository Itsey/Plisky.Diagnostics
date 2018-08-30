
namespace Plisky.Diagnostics.Listeners {
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    internal class AsyncTCPClient : IDisposable {
        private string status;

        // If debugging then with stop on throw then this pops up all the time if its failing to connect, have therefore
        // dropped it right down such that it only retries once every 5 minutes.
        private const int SECONDS_NO_SOCKET_RETRY = 60;
        private uint messagesWritten;
        private string m_ipAddress;
        private int m_port;
        private DateTime lastSocketException = DateTime.MinValue;
        private bool socketCommunicationsDown;
        private TcpClient m_tcpClient;
        private NetworkStream m_stream;

        public AsyncTCPClient(string ip, int port) {
            m_ipAddress = ip; m_port = port;
        }

        public async Task Writestufftest(string whatToWrite) {

            unchecked {
                messagesWritten++;
            }

            if ((socketCommunicationsDown) && (DateTime.Now - lastSocketException).TotalSeconds < SECONDS_NO_SOCKET_RETRY) {
                status = $"Socket coms down since {lastSocketException.ToString()}";
                Emergency.Diags.Log(status);
                return; 
            }
            socketCommunicationsDown = false;

            Byte[] data = Encoding.ASCII.GetBytes(whatToWrite);

            if ((m_tcpClient == null) || (!m_tcpClient.Connected)) {
                try {
                    m_tcpClient = new TcpClient(m_ipAddress, m_port);
                    m_stream = m_tcpClient.GetStream();
                } catch (SocketException sox) {
                    status = "EXX >> " + sox.Message;
                    Emergency.Diags.Log(status);
                    lastSocketException = DateTime.Now;
                    socketCommunicationsDown = true;
                    return;
                }
            }

            // Should now have reconnected or at least tried to connect.
            try {
                await m_stream.WriteAsync(data, 0, data.Length);
                //m_stream.Write(data, 0, data.Length);
                await m_stream.FlushAsync();
            } catch (IOException iox) {
                status = "EXX >> " + iox.Message;
                Emergency.Diags.Log(status);
                lastSocketException = DateTime.Now;
                socketCommunicationsDown = true;
                return;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // As Per Pattern: dispose managed state (managed objects).
                }

                if (m_stream != null) {
                    m_stream.Dispose();
                    m_stream = null;
                }

                if (m_tcpClient!=null) {
                    m_tcpClient.Close();
                    m_tcpClient = null;
                }

                // as per pattern: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // as per pattern: set large fields to null.

                disposedValue = true;
            }
        }

        internal string GetStatus() {
            return status + $"tried {messagesWritten}";
        }

        ~AsyncTCPClient() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // As Per Pattern: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}