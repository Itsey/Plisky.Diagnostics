using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

                } catch(SystemException) {
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
            b.AddHandler(new TCPHandler("127.0.0.1", 9060));

            Console.WriteLine("Actual Trace Level : "+b.ActiveTraceLevel.ToString());

            try {
                TraceSource ts = new TraceSource("monkeySwitch", SourceLevels.Off);
                TraceSwitch tx = new TraceSwitch("monkey2Switch", "test", "Off");

                SourceSwitch sw = new SourceSwitch("boner", "off");


                Console.WriteLine($"{ts.Switch.Level} >> {tx.Level} >> {sw.Level}");

                Console.ReadLine();
            } catch (Exception ex) {
                
            }
            return;

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
    }
}
