﻿using System;

namespace Plisky.Diagnostics.Test {
    class Program {
        static void Main(string[] args) {
            RegresssionTests rt = new RegresssionTests();
            rt.Prepare("Net FW 472 >> Application Online");
            rt.RunTests();

            rt.AllDone();

            Console.WriteLine("Press a Key To Exit");
            Console.ReadLine();
        }
    }
}
