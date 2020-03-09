namespace Plisky.Diagnostics.Listeners {

    using Plisky.Plumbing;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;

    public class DebugTCPHandler : TCPHandler {
        
        public Queue<string> TraceMessages = new Queue<string>();

        public void InternalLog(string what) {
            TraceMessages.Enqueue(what);
        }

        public DebugTCPHandler(string targetIp, int targetPort, bool harshFails = false): base(targetIp,targetPort,harshFails) {
            InternalLog($"Online {targetIp}:{targetPort}");
        }
    }
}

