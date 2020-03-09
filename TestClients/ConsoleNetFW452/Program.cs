using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plisky.Diagnostics.Test {
    class Program {
        static void Main(string[] args) {
            RegresssionTests rt = new RegresssionTests();
            rt.Prepare("Net FW 452 >> Application Online");
            rt.RunTests();

            rt.AllDone();

            Console.WriteLine("Press a Key To Exit");
            Console.ReadLine();
        }
    }
}
