using System;
using System.Collections.Generic;
using System.Text;

namespace Plisky.Diagnostics.Listeners {
 

    public class ConsoleFormatter : BaseMessageFormatter {

        protected override string ActualConvert(MessageMetadata msg) {
            string result = $"{DateTime.Now.ToString("HH:mm:ss")} ({msg.NetThreadId})>> {msg.Body}";
            return result;
        }

        
    }
}
