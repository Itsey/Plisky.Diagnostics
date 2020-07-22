using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Plisky.Diagnostics.Test {

    internal class MockMessageHandler : IBilgeMessageHandler {
        private List<MessageMetadata> allMessagesRecieved = new List<MessageMetadata>();

        private string ContextMustBe = null;
        private string MethodNameMustBe = null;

        public int LastMessageBatchSize { get; set; }

        public volatile int TotalMessagesRecieved;
        public string BodyMustContain { get; private set; }
        public string ProcessIdMustBe { get; private set; }
        public string ManagedThreadIdMustBe { get; private set; }

        public int Priority => 100;
        public string Name { get; set; }
        public int AssertionMessageCount = 0;

        public MockMessageHandler(string nme = nameof(MockMessageHandler)) {
            Name = nme;
            LastMessageBatchSize = 0;
        }

        public void HandleMessage(MessageMetadata md) {
            //Interlocked.Increment(ref TotalMessagesRecieved);
        }

        public void AssertAllConditionsMetForAllMessages(bool assertSomeMessagesRecieved = true, bool allowSingleMatch = false) {
            if (assertSomeMessagesRecieved) {
                Assert.True(TotalMessagesRecieved >0, "No messages written to the listener");
            }
            lock (allMessagesRecieved) {
                bool validMatchRecieved = false;

                foreach (var v in allMessagesRecieved) {
                    bool tmp =  ValidateMessageDataForMock(v, !allowSingleMatch);
                    if (!validMatchRecieved) {
                        validMatchRecieved = tmp;
                    }
                }
                if (allowSingleMatch) {
                    Assert.True(validMatchRecieved, "Not one of the messages matched the conditions");
                }
            }
        }

        private bool ValidateMessageDataForMock(MessageMetadata md, bool assertInline) {
            Assert.NotNull(md);

            if (ContextMustBe != null) {
                if (assertInline) {
                    Assert.Equal(ContextMustBe, md.Context);
                } else {
                    if (ContextMustBe != md.Context) {
                        return false;
                    }
                }
            }
            if (MethodNameMustBe != null) {
                if (assertInline) {
                    Assert.Equal(MethodNameMustBe, md.MethodName);
                } else {
                    if (MethodNameMustBe != md.MethodName) {
                        return false;
                    }
                }
            }

            if (BodyMustContain != null) {
                if (assertInline) {
                    Assert.True(md.Body.Contains(BodyMustContain), "The body did not contain the right info");
                } else {
                    if (!md.Body.Contains(BodyMustContain)) {
                        return false;
                    }
                }
            }
            if (ManagedThreadIdMustBe != null) {
                if (assertInline) {
                    Assert.Equal(ManagedThreadIdMustBe, md.NetThreadId);
                } else {
                    if (ManagedThreadIdMustBe != md.NetThreadId) {
                        return false;
                    }
                }

            }
            if (ProcessIdMustBe != null) {
                if (assertInline) {
                    Assert.Equal(ProcessIdMustBe, md.ProcessId);
                } else {
                    if (ProcessIdMustBe != md.ProcessId) {
                        return false;
                    }
                }
            }
            return true;
        }

        internal void AssertContextIs(string v) {
            ContextMustBe = v;
        }

        internal void SetMethodNameMustContain(string v) {
            MethodNameMustBe = v;
        }

        internal void SetMustContainForBody(string v) {
            BodyMustContain = v;
        }

        internal void AssertProcessId(int testProcId) {
            ProcessIdMustBe = testProcId.ToString();
        }

        internal void AssertManagedThreadId(int managedThreadId) {
            ManagedThreadIdMustBe = managedThreadId.ToString();
        }
         
        public Task HandleMessageAsync(MessageMetadata[] msg) {
            LastMessageBatchSize = msg.Length;
            foreach (var m in msg) {
                if (m.CommandType== TraceCommandTypes.AssertionFailed) {
                    AssertionMessageCount++;
                }
                TotalMessagesRecieved++;
                lock (allMessagesRecieved) {
                    allMessagesRecieved.Add(m);
                }

            }

            return Task.CompletedTask;
        }

        public void Flush() {
            
        }

        public void CleanUpResources() {
            
        }

        public string GetStatus() {
            return "hello";
        }

        public void HandleMessage40(MessageMetadata[] msg) {
            throw new NotImplementedException();
        }
    }
}