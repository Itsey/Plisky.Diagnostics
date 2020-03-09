

namespace Plisky.Diagnostics.Listeners {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class SimpleTraceFileHandler : IBilgeMessageHandler {
        private WeakReference lastTask;
        private FileStream fs;

        public PrettyReadableFormatter Formatter { get; private set; }

        public int Priority => 5;
        public string Name => nameof(SimpleTraceFileHandler);


        public async Task HandleMessageAsync(MessageMetadata[] msg) {
            var sb = new StringBuilder();
            foreach (var v in msg) {
                sb.Append(Formatter.ConvertToString(v));
            }
            byte[] txt = Encoding.UTF8.GetBytes(sb.ToString());
            var tsk = fs.WriteAsync(txt, 0, txt.Length);
            lastTask.Target = tsk;
            await tsk;
        }

        public void HandleMessage40(MessageMetadata[] msg) {
            StringBuilder sb = new StringBuilder();
            foreach (var v in msg) {
                sb.Append(Formatter.ConvertToString(v));
            }
            byte[] txt = Encoding.UTF8.GetBytes(sb.ToString());
            fs.Write(txt, 0, txt.Length);
        }




        public void Flush() {
            fs.Flush();
        }

        public void CleanUpResources() {
            if ((lastTask != null) && (lastTask.IsAlive)) {
                Task tsk = (Task)lastTask.Target;
                if (tsk != null) {
                    tsk.Wait();
                }
            }
            if (fs != null) {
                fs.Close();
            };
        }

        public string GetStatus() {
            return $"Writing to {TraceFilename}";
        }

        public string TraceFilename { get; private set; }
        /// <summary>
        /// This will create a simple text file listener designed for reading by a person, not for importing into
        /// a trace reader.
        /// </summary>
        /// <param name="pathForLog">The directory to place the file in (Defaults to %TEMP%)</param>
        /// <param name="overwriteEachTime">If set to true the same filename will be used, if false the current time will be appended</param>
        public SimpleTraceFileHandler(string pathForLog=null,bool overwriteEachTime=true) {
            string fn;
            if (string.IsNullOrEmpty(pathForLog)) {
                fn = Path.GetTempPath();
            } else {
                fn = pathForLog;
                if (!Directory.Exists(fn)) {
                    Directory.CreateDirectory(fn);
                }
            }

            if (!Directory.Exists(fn)) {
                throw new DirectoryNotFoundException(fn);
            }

            string actualFilename = "bilgedefault.log";
            if (!overwriteEachTime) {
                actualFilename = "bilgedefault" + DateTime.Now.ToString("ddMMyyyy_hhmmss") + ".log";
            }
            pathForLog = Path.Combine(fn, actualFilename);
            TraceFilename = pathForLog;
            fs = new FileStream(pathForLog, FileMode.Create);
            Formatter = new PrettyReadableFormatter();
        }
    }
}

