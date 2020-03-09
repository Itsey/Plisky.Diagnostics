

namespace Plisky.Diagnostics.Listeners {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

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
        public PrettyReadableFormatter Formatter { get; private set; }

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

        public InMemoryHandler(int maxDepth = 5000) {
            Formatter = new PrettyReadableFormatter();
            MaxQueueDepth = maxDepth;
        }

    }
}

