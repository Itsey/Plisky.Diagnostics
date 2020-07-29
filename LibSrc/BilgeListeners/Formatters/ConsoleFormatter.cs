using System;
using System.Collections.Generic;
using System.Text;

namespace Plisky.Diagnostics.Listeners {
 

    public class ConsoleFormatter : IMessageFormatter {

        public string ConvertToString(MessageMetadata msg) {
            string result;

            result = $"{DateTime.Now.ToString("HH:mm:ss")} ({msg.NetThreadId})>> {msg.Body}";

            return result;
        }
    }
}
