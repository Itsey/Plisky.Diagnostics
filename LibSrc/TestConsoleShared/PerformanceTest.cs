using Plisky.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Plisky.Diagnostics.Test {
    public class PerformanceTest {
        public Bilge b = new Bilge("module1","",tl: System.Diagnostics.SourceLevels.Verbose);
        private Dictionary<string, Stopwatch> clockers = new Dictionary<string, Stopwatch>();

        private  int MESSAGESTOSEND = 15000;

        public PerformanceTest() {


        }
        public void AnalysisBatchVsNoBatch() {
            int messageCount = 1000000;
            
            if (Debugger.IsAttached) {
                messageCount = 50;
            }

            for(int i=0; i<50; i++) {
                b.Info.Log("Warmup");
            }

            clockers.Add("analysis-off-batch", new Stopwatch());    // 368 no debugger 1m runs
            clockers.Add("analysis-small-batch", new Stopwatch());  // 348
            clockers.Add("analysis-large-batch", new Stopwatch());  // 345

            clockers["analysis-off-batch"].Start();
            PerformMessageTestBatchOff(messageCount);
            clockers["analysis-off-batch"].Stop();

            clockers["analysis-small-batch"].Start();
            PerformMessageTestBatchSmall(messageCount);
            clockers["analysis-small-batch"].Stop();

            clockers["analysis-large-batch"].Start();
            PerformMessageTestBatchLarge(messageCount);
            clockers["analysis-large-batch"].Stop();
        }

        private void PerformMessageTestBatchOff(int mc) {
            Bilge useThis = new Bilge();
            useThis.DisableMessageBatching();

            for(int i=0; i < mc; i++) {
                useThis.Info.Log($"Tis is the message that is averagely log {i}");
            }


        }

        private void PerformMessageTestBatchSmall(int mc) {
            Bilge useThis = new Bilge();
            useThis.SetMessageBatching(5,10000);

            for (int i = 0; i < mc; i++) {
                useThis.Info.Log($"Tis is the message that is averagely log {i}");
            }
        }

        private void PerformMessageTestBatchLarge(int mc) {
            Bilge useThis = new Bilge();
            useThis.SetMessageBatching(1000, 10000);

            for (int i = 0; i < mc; i++) {
                useThis.Info.Log($"Tis is the message that is averagely log {i}");
            }
        }

        public void ExecuteTest() {

            clockers.Clear();

            if (Debugger.IsAttached) {
                MESSAGESTOSEND = 100;
            }

            Func<Bilge> defaultBilge = () => {
                return b;
            };

            Func<Bilge> bigBatch = () => {
                var bx = new Bilge(tl: SourceLevels.Verbose);
                bx.SetMessageBatching(1000, 5000);
                return bx;

            };

            Func<Bilge> bilgeNoBatching = () => {
                Bilge bx = new Bilge(tl: SourceLevels.Verbose);
                bx.DisableMessageBatching();
                return bx;
            };


            Action<string, Action<Bilge>, Bilge> runPerformanceTest = (nameOfTheTest,testMethodToExecute,bilgeInstanceToUse) => {

                clockers.Add(nameOfTheTest, new Stopwatch());
                Bilge bToUse = bilgeInstanceToUse;
                clockers[nameOfTheTest].Start();

                testMethodToExecute(bToUse);

                clockers[nameOfTheTest].Stop();
            };

            

            clockers.Add("main", new Stopwatch());
            clockers.Add("main-noflush", new Stopwatch());

            clockers["main"].Start();
            clockers["main-noflush"].Start();

            runPerformanceTest("singleLog-default",StandardLogOnMainThread,defaultBilge());
            runPerformanceTest("singleLog-nobatch", StandardLogOnMainThread, bilgeNoBatching());
            runPerformanceTest("singleLog-bigbatch", StandardLogOnMainThread, bigBatch());
            //runPerformanceTest("standard-onelog-nobatch", StandardLogOnMainThread, bilgeNoBatching());
            runPerformanceTest("mutilog-default", MultiLogOnMainThread, defaultBilge());


            clockers["main-noflush"].Stop();
            b.Flush();
            clockers["main"].Stop();

            
        }

        public void WriteOutput() {
            foreach (var l in clockers.Keys) {
                Console.WriteLine($"{l} : {clockers[l].ElapsedMilliseconds}");
            }

            Console.WriteLine($"Done");
            Console.ReadLine();
        }



        private void StandardLogOnMainThread(Bilge useThis) {
            for (int i = 0; i < MESSAGESTOSEND; i++) {
                useThis.Info.Log($"ITs a random message {i}");
            }
        }

        private void MultiLogOnMainThread(Bilge useThis) {
            for (int i = 0; i < MESSAGESTOSEND; i++) {
                useThis.Info.Log($"ITs a random message {i}");
                useThis.Verbose.Log($"Again {i}", "With sEcondary");
            }
            
        }
    }



}
