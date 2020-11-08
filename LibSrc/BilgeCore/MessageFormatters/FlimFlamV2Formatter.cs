namespace Plisky.Diagnostics {
    using Plisky.Plumbing;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    

    public class FlimFlamV2Formatter : BaseMessageFormatter {
        
        protected string EscapeString(string input) {
            return input.Replace("\n", "\\n").Replace("\"", "\\\"").Replace("\\", "\\\\");
        }

        
        protected override string DefaultConvertWithReference(MessageMetadata msg, string uniquenessReference) {
            return ConvertMsg(msg, uniquenessReference);
        }

        protected override string ActualConvert(MessageMetadata msg) {
            return ConvertMsg(msg, DEFAULT_UQR);
        }

        private string ConvertMsg(MessageMetadata msg, string defaultUqr) {
            msg.NullsToEmptyStrings();

            string result;

            try {
                string ald = msg.ClassName + "::" + msg.MethodName;
                string metaPart = $"\"v\":\"2\",\"uq\":\"{defaultUqr}\"";
                string cnamePart = $"\"c\":\"{EscapeString(msg.ClassName)}\",\"l\":\"{msg.LineNumber}\",\"mn\":\"{EscapeString(msg.MethodName)}\",\"md\":\"{EscapeString(msg.FileName)}\",\"al\":\"{EscapeString(ald)}\"";
                string procPart = $"\"nt\":\"{msg.NetThreadId}\",\"p\":\"{msg.ProcessId}\",\"t\":\"{msg.OSThreadId}\",\"man\":\"{EscapeString(msg.MachineName)}\"";
                string msgPart = $"\"m\":\"{EscapeString(msg.Body)}\",\"s\":\"{EscapeString(msg.FurtherDetails)}\",\"mt\":\"{TraceCommands.TraceCommandToString(msg.CommandType)}\"";
                result = $"{{ {metaPart},{cnamePart},{procPart},{msgPart} }}";
            } catch (Exception ex) {
                Emergency.Diags.Log("EXX >> " + ex.Message);
                throw;
            }
            return result;
        }
    }

   
}