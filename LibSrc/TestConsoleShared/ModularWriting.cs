using Plisky.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plisky.Diagnostics.Test {
    public class ModularWriting {
        public Module1 m1 = new Module1();
        public Module2 m2 = new Module2();
        public Module3 m3 = new Module3();


        public ModularWriting() {

        }


        public void DoWrite() {
            for(int i=0; i<10; i++) {
                m1.WriteStuff();
            }
        }
    }


    public class Module1 {
        public Bilge b = new Bilge("module1");

        internal void WriteStuff() {
            b.Info.Log("InfoLevelWriting Module 1");
            b.Verbose.Log("Verbose LevelWriting Module 1");
            b.Error.Log("Error LevelWriting Module 1");
        }
    }


    public class Module2 {
        public Bilge b = new Bilge("module2");

        internal void WriteStuff() {
            b.Info.Log("InfoLevelWriting Module 2");
            b.Verbose.Log("Verbose LevelWriting Module 2");
            b.Error.Log("Error LevelWriting Module 2");
        }
    }

    public class Module3 {
        public Bilge b = new Bilge("module3");

        internal void WriteStuff() {
            b.Info.Log("InfoLevelWriting Module 3");
            b.Verbose.Log("Verbose LevelWriting Module 3");
            b.Error.Log("Error LevelWriting Module 3");
        }
    }
}
