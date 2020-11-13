

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
    public class InMemoryHandler : BaseHandler, IBilgeMessageHandler {
        protected Queue<string> messages = new Queue<string>();
        private uint uniqueMessageIndex;


        public int MaxQueueDepth { get; set; }

        /// <summary>
        /// Returns the oldest message, removing it from the queue if clear on get is set.
        /// </summary>
        /// <returns></returns>
        public string GetMessage() {
            if (messages.Count == 0) {
                return null;
            }

            return ClearOnGet ? messages.Dequeue() : messages.Peek();

        }

        public string[] GetAllMessages() {

            var exm = messages;
            if (ClearOnGet) {
                messages = new Queue<string>();
            }

            return exm.ToArray();

        }

        
        public string Name => nameof(InMemoryHandler);
        public bool ClearOnGet { get; set; }

        public async Task HandleMessageAsync(MessageMetadata[] msg) {

            var sb = new StringBuilder();
            foreach (var v in msg) {
                uniqueMessageIndex++;
                sb.Append(Formatter.ConvertWithReference(v, uniqueMessageIndex.ToString()));
                messages.Enqueue(sb.ToString());
                if (messages.Count > MaxQueueDepth) {
                    _ = messages.Dequeue();
                }
                sb.Clear();
            }


        }

        public void HandleMessage40(MessageMetadata[] msg) {
            var sb = new StringBuilder();
            foreach (var v in msg) {
                uniqueMessageIndex++;
                sb.Append(Formatter.ConvertWithReference(v, uniqueMessageIndex.ToString()));
                messages.Enqueue(sb.ToString());
                if (messages.Count > MaxQueueDepth) {
                    _ = messages.Dequeue();
                }
                sb.Clear();
            }
        }

        public int GetMessageCount() {
            return messages.Count;
        }

     
        public string GetStatus() {
            return $"writing ok, current depth {GetMessageCount()} maxDepth {MaxQueueDepth}";
        }

        
        public InMemoryHandler(InMemoryHandlerOptions imho): this() {
            if (imho!=null) {
                MaxQueueDepth = imho.MaxQueueDepth;
                ClearOnGet = imho.ClearOnGet;
            }

        }
        public InMemoryHandler(int maxDepth = 5000) {
            Formatter = DefaultFormatter(false);
            MaxQueueDepth = maxDepth;
            ClearOnGet = true;
        }

    }
}

