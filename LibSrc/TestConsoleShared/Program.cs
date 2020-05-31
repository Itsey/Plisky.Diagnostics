using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if CORE
using Microsoft.Extensions.Configuration;
#endif

namespace Plisky.Diagnostics.Test {
    class Program {
        static void Main(string[] args) {

            Bilge.AddMessageHandler(new TCPHandler("127.0.0.1", 9060));
            Bilge.Alert.Online("TestClient");


            PerformanceTest p = new PerformanceTest();
            p.AnalysisBatchVsNoBatch();
            //p.ExecuteTest();
            p.WriteOutput();
            

            Bilge.ForceFlush();
            return; 

#if CORE
            // Due to the dependency on configurations and the different ways that you can configure a core service this is not
            // implemented within Bilge even as a default but is documented instead.
            Bilge.SetConfigurationResolver((s, lvl) => {
                var configuration = new ConfigurationBuilder()
                  .AddJsonFile("appsettings.json", false, true)
                 .Build();

                SourceLevels result = lvl;
                string defaultValue = configuration["logging:loglevel:default"];
                string specificForThisInstance = configuration[$"logging:loglevel:{s}"];                

                if (Enum.TryParse<SourceLevels>(specificForThisInstance, out SourceLevels slSpecific)) {
                    result = slSpecific;                    
                } else {
                    if (Enum.TryParse<SourceLevels>(defaultValue, out SourceLevels slDefault)) {
                        result = slDefault;                        
                    }
                }

                return result;

            });

#else 
            Bilge.SetConfigurationResolver((instanceName, lvl) => {

                // Logic -> Try Source Switch, Failing that Trace Switch, failing that SourceSwitch + Switch, Failing that TraceSwitch+Switch.

                SourceLevels result = lvl;
                bool tryTraceSwitches = true;

                try {
                    SourceSwitch ss = new SourceSwitch(instanceName);
                    if (ss.Level == SourceLevels.Off) {
                        ss = new SourceSwitch($"{instanceName}Switch");
                        if (ss.Level == SourceLevels.Off) {

                        } else {
                            tryTraceSwitches = false;
                            result = ss.Level;
                        }
                    } else {
                        tryTraceSwitches = false;
                        result = ss.Level;
                    }

                } catch (SystemException) {
                    // This is the higher level exception of a ConfigurationErrorsException but that one requires a separate reference
                    // This occurs when a TraceSwitch has the same name as the source switch with a value that is not supported by source switch e.g. Info

                }

                if (tryTraceSwitches) {
                    TraceSwitch ts = new TraceSwitch(instanceName, "");
                    if (ts.Level == TraceLevel.Off) {
                        ts = new TraceSwitch($"{instanceName}Switch", "");
                        if (ts.Level != TraceLevel.Off) {
                            result = Bilge.ConvertTraceLevel(ts.Level);
                        }
                    } else {
                        result = Bilge.ConvertTraceLevel(ts.Level);
                    }
                }

                return result;

            });

#endif

            Bilge b = new Bilge("PliskyConsoleTestApp");
            b.CurrentTraceLevel = TraceLevel.Verbose;


            b.Verbose.Log("Hello Cruel World");

            bool traceSwitchTests = false;
            bool bulkFileWriteTests = true;

            if (bulkFileWriteTests) {
                ProductionLoggingTest(b);
            }
            b.Flush();
            return;
           
            b.AddHandler(new SimpleTraceFileHandler(@"c:\temp\"));
            if (traceSwitchTests) {
                Console.WriteLine("Actual Trace Level : " + b.ActiveTraceLevel.ToString());

                try {
                    TraceSource ts = new TraceSource("monkeySwitch", SourceLevels.Off);
                    TraceSwitch tx = new TraceSwitch("monkey2Switch", "test", "Off");

                    SourceSwitch sw = new SourceSwitch("boner", "off");


                    Console.WriteLine($"{ts.Switch.Level} >> {tx.Level} >> {sw.Level}");

                    Console.ReadLine();
                } catch (Exception ex) {

                }
            }

            bool doDirectWriting = false;

            if (args.Length > 0) {
                if (args[0] == "direct") {
                    doDirectWriting = true;
                }

            }

            if (doDirectWriting) {
                var dr = new DirectWriting(b);
                dr.DoDirectWrites();
            }



            ModularWriting mr = new ModularWriting();
            mr.DoWrite();
            b.Flush();
            Console.WriteLine("Readline");
            Console.ReadLine();
        }

        private static int runCount=0;
        private static void ProductionLoggingTest(Bilge b) {
            string fn = $"Log{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}.Log";
            fn = Path.Combine(@"D:\Temp\_DelWorking\", fn);
               
                
            b.AddHandler(new FileSystemHandler(new FSHandlerOptions(fn)));

            for (int t = 0; t < 9; t++) {
                ThreadPool.QueueUserWorkItem(o => {
                    Interlocked.Increment(ref runCount);
                    for (int i = 0; i < 100000; i++) {
                        b.Info.Log($"hello world number {i}");
                        for (int j = 0; j < 10; j++) {
                            b.Verbose.Log($"hello verbose world {j}");
                        }
                    }
                    Interlocked.Decrement(ref runCount);
                });
            }

            for (int i = 0; i < 100000; i++) {
                b.Info.Log($"hello world number {i}");
                for (int j = 0; j < 10; j++) {
                    b.Verbose.Log($"hello verbose world {j}");
                }
            }
            while (runCount > 0) {
                Thread.Sleep(0);
            }
        }
    }
}
