using System;

namespace Plisky.Diagnostics.Listeners {

    public class PrettyReadableFormatter : IMessageFormatter {

        public string ConvertToString(MessageMetadata msg) {
            string result;

            result = $"{DateTime.Now} >> {msg.Body} <<|>> {msg.ClassName}@{msg.LineNumber} in {msg.ProcessId}@{msg.MachineName} Thread[{msg.NetThreadId},{msg.OSThreadId}] ({msg.FurtherDetails}) [{msg.CommandType.ToString()}] {Environment.NewLine}";

            return result;
        }
    }
}