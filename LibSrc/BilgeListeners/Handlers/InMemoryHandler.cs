

namespace Plisky.Diagnostics.Listeners {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class InMemoryHandler : IBilgeMessageHandler {
        private int messageIndex;
        private Queue<string> messages = new Queue<string>();

        public string GetMessage() {
            return messages.Dequeue();
        }

        public IEnumerable<string> GetAllMessages() {
            while (messages.Count > 0) {
                yield return messages.Dequeue();
            }
        }
        public PrettyReadableFormatter Formatter { get; private set; }

        public int Priority => 5;
        public string Name => nameof(InMemoryHandler);

#if NET452 || NETSTANDARD2_0
        public async Task HandleMessageAsync(MessageMetadata[] msg) {

            var sb = new StringBuilder();
            foreach (var v in msg) {
                sb.Append($"{messageIndex++} - ");
                sb.Append(Formatter.ConvertToString(v));
            }
            messages.Enqueue(sb.ToString());

        }
#else
            public void HandleMessage40(MessageMetadata[] msg) {
            throw new NotImplementedException();
        }
#endif


        public void Flush() {

        }



        public void CleanUpResources() {

        }

    }
}

