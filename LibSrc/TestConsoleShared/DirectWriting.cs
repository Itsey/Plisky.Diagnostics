using Plisky.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Plisky.Diagnostics.Test {
    public class DirectWriting {
        public Bilge b;

        public DirectWriting(Bilge bn) {
            b = bn;
        }


        public void DoDirectWrites() {
            for (int j = 0; j < 10; j++) {
                b.Direct.Write("Direct", $"Direct {j} > {DateTime.Now.ToString()}");
                Thread.Sleep(500);
            }


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
                b.Direct.Write("[STAT][Loop2]", $"{i * 10}");
                Console.WriteLine(msg);
            }

        }

    }
}
