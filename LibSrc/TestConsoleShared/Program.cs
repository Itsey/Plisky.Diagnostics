using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsole {
    class Program {
        static void Main(string[] args) {
            Bilge b = new Bilge(tl:TraceLevel.Verbose);
            b.AddHandler(new TCPHandler("127.0.0.1", 9060));
            //b.Info.Log("Info");
            //b.Verbose.Log("Verbose");
            //b.Warning.Log("Warning");
            //b.Error.Log("Error");

            for (int j = 0; j < 10; j++) {              
                b.Direct.Write("Direct", $"Direct {j} > {DateTime.Now.ToString()}");
                Thread.Sleep(500);
            }

            if (true) {
                for (int i = 0; i < 1000; i++) {
                    Console.WriteLine("Senidng");
                    Thread.Sleep(50);
                    string mid = "X";
                    switch (i % 10) {
                        case 1: mid = "X         "; break;
                        case 2: mid = " X        "; break;
                        case 3: mid = "  X       "; break;
                        case 4: mid = "   X      "; break;
                        case 5: mid = "    X     "; break;
                        case 6: mid = "     X    "; break;
                        case 7: mid = "      X   "; break;
                        case 8: mid = "       X  "; break;
                        case 9: mid = "        X "; break;
                        case 0: mid = "         X"; break;
                    }
                    string msg = $"XXXXXXXXXX|XXXXXXXXXX|XXX    XXX|{mid}|XXX    XXX|XXXXXXXXXX";
                    b.Direct.Write("[GRID][Map]", msg);
                    b.Direct.Write("[STAT][Loop1]", $"{i}");
                    b.Direct.Write("[STAT][Loop2]", $"{i*10}");
                    Console.WriteLine(msg);
                }
            }
            b.Flush();
            Console.WriteLine("Readline");
            Console.ReadLine();
        }
    }
}
