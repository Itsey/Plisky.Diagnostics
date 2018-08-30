

namespace Plisky.Diagnostics.Listeners {
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class FileSystemHandler : IBilgeMessageHandler {
        private string filePath;
        private string status;
        private FileStream fs;

        public LegacyFlimFlamFormatter Formatter { get; private set; }

        public int Priority => 5;
        public string Name => nameof(FileSystemHandler);

#if NET452 || NETSTANDARD2_0
        public async Task HandleMessageAsync(MessageMetadata[] msg) {
            try {
                StringBuilder sb = new StringBuilder();
                foreach (var v in msg) {
                    sb.Append(Formatter.ConvertToString(v));
                }
                byte[] txt = Encoding.UTF8.GetBytes(sb.ToString());
                await fs.WriteAsync(txt, 0, txt.Length);
                status = "ok";
            } catch (Exception ex) {
                status = ex.Message;
                throw;
            }
            
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
            filePath = path + "output.log";
            fs = new FileStream(filePath, FileMode.Create);
        }

        public void CleanUpResources() {
            if (fs != null) {
                fs.Close();
                fs = null;
            }
        }

        public string GetStatus() {
            return $"writing to {filePath} last status {status}";
        }
    }
}

