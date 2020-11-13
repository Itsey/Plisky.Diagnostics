

namespace Plisky.Diagnostics.Listeners {
    using Plisky.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class ConsoleHandler : BaseHandler, IBilgeMessageHandler {
        
        public string Name => nameof(ConsoleHandler);

        public async Task HandleMessageAsync(MessageMetadata[] msg) {

            foreach (var v in msg) {
                if (WriteConsolePreamble(v.CommandType)) {
                    string writer = Formatter.Convert(v);
                    Console.WriteLine(writer);
                }                
            }


        }

        private bool WriteConsolePreamble(TraceCommandTypes commandType) {
            bool doWrite = true;
            string preamble = "";
            ConsoleColor cc = ConsoleColor.White;

            switch (commandType) {
                                
                case TraceCommandTypes.LogMessageVerb:
                    cc = ConsoleColor.Blue;
                    preamble = "Verb";
                    break;
                case TraceCommandTypes.LogMessage:
                case TraceCommandTypes.LogMessageMini:
                    cc = ConsoleColor.Green;
                    preamble = "Info";
                    break;
                case TraceCommandTypes.SectionStart:
                case TraceCommandTypes.SectionEnd:
                case TraceCommandTypes.ResourceEat:
                case TraceCommandTypes.ResourcePuke:
                case TraceCommandTypes.ResourceCount:
                case TraceCommandTypes.Standard:
                case TraceCommandTypes.CommandXML:
                case TraceCommandTypes.Custom:
                case TraceCommandTypes.MoreInfo:
                case TraceCommandTypes.CommandOnly:
                case TraceCommandTypes.InternalMsg:                    
                case TraceCommandTypes.TraceMessageIn:                    
                case TraceCommandTypes.TraceMessageOut:                    
                case TraceCommandTypes.TraceMessage:
                    doWrite = false;
                    break;
                case TraceCommandTypes.AssertionFailed:
                    cc = ConsoleColor.Blue;
                    preamble = "Assert";
                    break;               
                case TraceCommandTypes.ErrorMsg:
                    cc = ConsoleColor.Red;
                    preamble = "Error";
                    break;
                case TraceCommandTypes.WarningMsg:
                    cc = ConsoleColor.DarkYellow;
                    preamble = "Warn";
                    break;
                case TraceCommandTypes.ExceptionBlock:                    
                case TraceCommandTypes.ExceptionData:
                case TraceCommandTypes.ExcStart:
                case TraceCommandTypes.ExcEnd:
                    cc = ConsoleColor.DarkMagenta;
                    preamble = "Fault";
                    break;               
                case TraceCommandTypes.Alert:
                    cc = ConsoleColor.Blue;
                    preamble = "Alert";
                    break;
                case TraceCommandTypes.Unknown:
                    doWrite = false;
                    break;                
            }

            if (doWrite) {
                Console.ForegroundColor = cc;
                Console.Write(preamble);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" : ");
            }
            return doWrite;

        }

        public void HandleMessage40(MessageMetadata[] msg) {
            foreach (var l in msg) {
                Console.WriteLine(msg);
            }
        }

      
        public string GetStatus() {
            return $"writing ok";
        }

        public ConsoleHandler() {
            Formatter = new ConsoleFormatter();
            this.Priority = 10;

        }

    }
}

