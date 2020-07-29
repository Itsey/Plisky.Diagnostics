using Plisky.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plisky.Diagnostics.Test {
    public class ModularWriting {
        Bilge b = new Bilge("outerModule");
        Module[] mods;





        public ModularWriting(int modCount = 3) {
            mods = new Module[modCount];
            for (int i = 0; i < modCount; i++) {
                mods[i] = new Module(modCount.ToString());
            }

        }


        public void DoWrite() {
            for (int i = 0; i < 100; i++) {
                foreach(var mo in mods) {
                    mo.WriteStuff();
                }
            }

        }
    }


    public class Module {
        protected string name;
        public Bilge b = new Bilge("module1");

        internal void WriteStuff() {
            b.Info.Log($"InfoLevelWriting {name}");
            b.Verbose.Log($"Verbose LevelWriting {name}");
            b.Error.Log($"Error LevelWriting {name}");
            b.Warning.Log($"Warning level writing {name}");
            b.Error.Dump(new Exception("Outer Error", new Exception("Inner Error")), $"Exception written {name}");
        }

        public Module(string nm) {
            name = nm;

        }
    }


}
