using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole {
    class Program {
        static void Main(string[] args) {
            Bilge b = new Bilge(tl:TraceLevel.Verbose);
            b.AddHandler(new TCPHandler("127.0.0.1", 9060));
            b.Info.Log("Info");
            b.Verbose.Log("Verbose");
            b.Warning.Log("Warning");
            b.Error.Log("Error");

            Console.ReadLine();
        }
    }
}
