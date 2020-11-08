using System;

namespace Plisky.Diagnostics {

    public class PrettyReadableFormatter : BaseMessageFormatter {

        protected override string ActualConvert(MessageMetadata msg) {
            string result;

            result = $"{DateTime.Now} >> {msg.Body} <<|>> {msg.ClassName}@{msg.LineNumber} in {msg.ProcessId}@{msg.MachineName} Thread[{msg.NetThreadId},{msg.OSThreadId}] ({msg.FurtherDetails}) [{msg.CommandType.ToString()}] {Environment.NewLine}";

            return result;
        }

        
    }
}