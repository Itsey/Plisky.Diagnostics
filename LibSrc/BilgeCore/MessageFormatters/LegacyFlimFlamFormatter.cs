namespace Plisky.Diagnostics {
    using Plisky.Plumbing;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class LegacyFlimFlamFormatter : BaseMessageFormatter {
        protected override string ActualConvert(MessageMetadata msg) {
        
            string result;
            MessageParts nextMsg = new MessageParts();
            msg.NullsToEmptyStrings();
            // These will cause the stream to be written if we are queueingf messages.
            //if ((messageType == Constants.ASSERTIONFAILED) || (theMessage == Constants.EXCEPTIONENDTAG) || (messageType == Constants.ERRORMSG)) {
            //    nextMsg.TriggerRefresh = true;
            //}
            Emergency.Diags.Log("Formatting string");
            try {


                nextMsg.DebugMessage = msg.Body;
                nextMsg.SecondaryMessage = msg.FurtherDetails;
                nextMsg.ClassName = msg.ClassName;
                nextMsg.lineNumber = msg.LineNumber;
                nextMsg.MethodName = msg.MethodName;
                nextMsg.MachineName = msg.MachineName;
                nextMsg.netThreadId = msg.NetThreadId;
                nextMsg.osThreadId = msg.OSThreadId;
                nextMsg.ProcessId = msg.ProcessId;
                //nextMsg.ModuleName = Path.GetFileNameWithoutExtension(msg.FileName);
                nextMsg.ModuleName = msg.FileName;
                nextMsg.AdditionalLocationData = msg.ClassName + "::" + msg.MethodName;

                // Populate Message Type
                nextMsg.MessageType = TraceCommands.TraceCommandToString(msg.CommandType);

                result = TraceMessageFormat.AssembleTexStringFromPartsStructure(nextMsg);
            } catch ( Exception ex) {
                Emergency.Diags.Log("EXX >> "+ex.Message);
                throw;
            }
            Emergency.Diags.Log("REturning legacvy string"+result);
            return result;
        }
    }
}