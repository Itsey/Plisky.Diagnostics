#if false
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// Needs to migrate to interfaces
namespace Plisky.Plumbing.Listeners {
    public class StorageListener : BaseListener {
        public List<string> Messages = new List<string>();

        public override void Write(string message) {
            Messages.Add(message);
        }

        public override void WriteLine(string message) {
            Messages.Add(message);
        }

        protected override string GetListenerName() {
            return "StorgageListener";
        }

        protected override string AdditionalDiagnosticInfo() {
            return "MessageCount:" + Messages.Count.ToString();
        }
    }
}
#endif