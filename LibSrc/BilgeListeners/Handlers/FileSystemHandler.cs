

namespace Plisky.Diagnostics.Listeners {
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class FileSystemHandler : IBilgeMessageHandler, IDisposable {
        private FileStream fs;

        public LegacyFlimFlamFormatter Formatter { get; private set; }

        public int Priority => 5;
        public string Name => nameof(FileSystemHandler);

#if NET452 || NETSTANDARD2_0
        public async Task HandleMessageAsync(MessageMetadata[] msg) {
            StringBuilder sb = new StringBuilder();
            foreach (var v in msg) {
                sb.Append(Formatter.ConvertToString(v));
            }
            byte[] txt = Encoding.UTF8.GetBytes(sb.ToString());
            await fs.WriteAsync(txt, 0, txt.Length);
        }
#else
        public void HandleMessage40(MessageMetadata[] msg) {
            throw new NotImplementedException();
        }
#endif


        public void Flush() {
            fs.Flush();
        }

        public FileSystemHandler(string path) {
            path += "output.log";
            fs = new FileStream(path, FileMode.Create);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects).

                }

                // As Per Template: free unmanaged resources (unmanaged objects) and override a finalizer below.
                if (fs != null) {
                    fs.Close();
                    fs = null;
                }

                // As Per Pattern: set large fields to null.

                disposedValue = true;
            }
        }


         ~FileSystemHandler() {
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

