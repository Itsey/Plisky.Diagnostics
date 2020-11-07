

namespace Plisky.Diagnostics.Listeners {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// InMemoryHandler is, as suggested by the name a handler that stores the log data in memory to be retireved later.  Therefore it does not actually
    /// write the data anyhwere.  It is up to the caller to retrieve the messages.  As such you should configure maximum queue depths to ensure that not
    /// too many resources are consumed by old logs.
    /// </summary>
    public class InMemoryHandler : IBilgeMessageHandler {

        private int messageIndex;
        private Queue<string> messages = new Queue<string>();

        public int MaxQueueDepth { get; set; }

        public string GetMessage() {
            return messages.Dequeue();
        }

        public string[] GetAllMessages() {
            int currentCount = messages.Count;
            string[] result = new string[messages.Count];
            for(int i=0; i< currentCount; i++) {
                result[i] = GetMessage();
            }
            return result;
        }
        public IMessageFormatter Formatter { get; private set; }

        public int Priority => 5;
        public string Name => nameof(InMemoryHandler);

        public async Task HandleMessageAsync(MessageMetadata[] msg) {

            var sb = new StringBuilder();
            foreach (var v in msg) {
                sb.Append($"{messageIndex++} - ");
                sb.Append(Formatter.ConvertToString(v));
                messages.Enqueue(sb.ToString());
                if (messages.Count>MaxQueueDepth) {
                    _ = messages.Dequeue();
                }
                sb.Clear();
            }
            

        }

            public void HandleMessage40(MessageMetadata[] msg) {
            throw new NotImplementedException();
        }

        public int GetMessageCount() {
            return messages.Count;
        }

        public void Flush() {
        }

        public void CleanUpResources() {
        }

        public string GetStatus() {
            return $"writing ok, current depth {GetMessageCount()} maxDepth {MaxQueueDepth}";
        }

        public void SetFormatter(IMessageFormatter fmt) {
            if (fmt != null) {
                Formatter = fmt;
            }
        }

        public InMemoryHandler(int maxDepth = 5000) {
            Formatter = new PrettyReadableFormatter();
            MaxQueueDepth = maxDepth;
        }

    }
}

