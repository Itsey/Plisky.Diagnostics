using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Plisky.Diagnostics {



    public class Emergency {
        private static string filename = null;
        private static List<string> allMessages = new List<string>();
        public static Emergency Diags = new Emergency();

        [Conditional("ACTIVEDEBUG")]
        public void Shutdown() {
#if ACTIVEDEBUG
            if (filename == null) {
                string fn = Path.GetTempPath();

                fn = Path.Combine(fn, $"bilgeelg.txt");
                File.WriteAllText(fn, $"Bilge Log At {DateTime.Now.Date.ToString()} at {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second} {Environment.NewLine}");
                if (filename == null) {
                    filename = fn;
                }
            }

            // This is nasty, we have no way of knowing if they have written to our store
            Thread.Sleep(200);
            lock (allMessages) {
                foreach (var v in allMessages) {
                    File.AppendAllText(filename, v);

                }
            }
#endif
        }
        [Conditional("ACTIVEDEBUG")]
        public void Log(string msg) {
#if ACTIVEDEBUG
            msg = $"{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second} >> {msg} {Environment.NewLine}";

            lock (allMessages) {
                allMessages.Add(msg);
            }


#endif
        }
    }


}