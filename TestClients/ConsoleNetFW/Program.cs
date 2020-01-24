using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleNetFW {
    class Program {
        static void Main(string[] args) {
            var b = new Bilge(tl: System.Diagnostics.TraceLevel.Verbose);
            b.AddHandler(new TCPHandler("127.0.0.1", 9060));
            b.
            b.Info.Log("Application Online");

            ExampleSubsystem.Run(b);

            ExampleSubsystem.Run2(b);

            b.Info.Log("All Done.");
            b.Flush();

            Console.WriteLine("Press a Key To Exit");
            Console.ReadLine();
        }
    }
}
