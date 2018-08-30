using System;
using System.Threading;
using Plisky.Diagnostics;

namespace ConsoleNetFW {
    internal class ExampleSubsystem {
        private static ARepositry vsr1;
        private static ARepositry vsr2;

        internal static void Run(Bilge b) {
            b.Info.Log("Example Subsystem online");

            vsr1 = new VerySlowRepository();
            vsr2 = new FastRepository();

            for (int i = 0; i < 10; i++) {
                vsr1.DoSomeWork();
                vsr2.DoSomeWork();
            }


        }

        internal static void Run2(Bilge b) {
            string timerName = "sampleRun";
            string sinkName1 = "WebService";
            string sinkName2 = "DataBase";
            Random r = new Random();
            
            for (int i = 0; i < 100; i++) {
                b.Verbose.TimeStart(timerName, timerCategoryName: sinkName1);
                Thread.Sleep(r.Next(10));
                b.Verbose.TimeStop(timerName, timerCategoryName: sinkName1);

                b.Verbose.TimeStart(timerName, timerCategoryName: sinkName2);
                Thread.Sleep(r.Next(20));
                b.Verbose.TimeStop(timerName, timerCategoryName: sinkName2);
            }

        }
    }

   
}