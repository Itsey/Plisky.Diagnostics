using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plisky.Diagnostics.Test {
    class Program {
        static void Main(string[] args) {
#if false
            RegresssionTests rt = new RegresssionTests();
            rt.Prepare("Net FW 35 >> Application Online");
            rt.RunTests();

            rt.AllDone();

            Console.WriteLine("Press a Key To Exit");
            Console.ReadLine();
#endif
        }
    }
}
