using Plisky.Plumbing;
using System.Threading;
using System;

namespace Plisky.Diagnostics {

    public class MessageMetadata {
        private static long baseIndex = 0;

        public long Index { get; private set; }

        public MessageMetadata() {
            Index = Interlocked.Increment(ref baseIndex);
        }

        public MessageMetadata(string meth, string pth, int ln) {
            MethodName = meth;
            FileName = pth;
            LineNumber = ln.ToString();
        }

        public TraceCommandTypes CommandType { get; set; }

        public string Context { get; set; }
        public string MethodName { get; set; }
        public string FileName { get; set; }
        public string LineNumber { get; set; }
        public string Body { get; set; }
        public string FurtherDetails { get; set; }
        public string MachineName { get; set; }
        public string ProcessId { get; set; }
        public string NetThreadId { get; set; }
        public string OSThreadId { get; set; }
        public string ClassName { get; set; }

        public MessageMetadata Clone() {
            return new MessageMetadata() {
                CommandType = this.CommandType,
                Context = this.Context,
                MethodName = this.MethodName,
                FileName = this.FileName,
                LineNumber = this.LineNumber,
                Body = this.Body,
                FurtherDetails = this.FurtherDetails,
                MachineName = this.MachineName,
                ProcessId = this.ProcessId,
                NetThreadId = this.NetThreadId,
                OSThreadId = this.OSThreadId,
                ClassName = this.ClassName
            };
        }

        public void NullsToEmptyStrings() {
            Context = Context ?? "";
            MethodName = MethodName ?? "";
            FileName = FileName ?? "";
            LineNumber = LineNumber ?? "";
            Body = Body ?? "";
            FurtherDetails = FurtherDetails ?? "";
            MachineName = MachineName ?? "";
            ProcessId = ProcessId ?? "";
            NetThreadId =NetThreadId ?? ""; 
            OSThreadId = OSThreadId ?? ""; 
            ClassName = ClassName ?? "";
        }
    }
}