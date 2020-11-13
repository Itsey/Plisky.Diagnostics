using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Plisky.Diagnostics.Listeners {
    public class FileSystemHandler : BaseHandler, IBilgeMessageHandler {
        private FSHandlerOptions opt;
        protected FileStream fs;
        

        public FileSystemHandler(FSHandlerOptions sLFHandlerOptions) {
            this.opt = sLFHandlerOptions;
            Formatter = DefaultFormatter(false);
            Name = nameof(FileSystemHandler);
        }

        
        public string Name { get; }

        
        public string GetStatus() {
            return "OK";
        }

        public void HandleMessage40(MessageMetadata[] msg) {
            HandleMessageAsync(msg).Wait();
        }

        public async Task HandleMessageAsync(MessageMetadata[] msg) {

            fs = new FileStream(opt.FileName, FileMode.Append, FileAccess.Write);

            
            try {

                var sb = new StringBuilder();
                for (int i = 0; i < msg.Length; i++) {
                    sb.Append(Formatter.Convert(msg[i]));
                    sb.Append(Environment.NewLine);
                }

                var data = Encoding.ASCII.GetBytes(sb.ToString().ToCharArray());
                await fs.WriteAsync(data, 0, data.Length);
            } finally {
                fs.Close();
            }
        }

    

    }
}
