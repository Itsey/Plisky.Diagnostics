namespace Plisky.Diagnostics.Test {

    using Plisky.Diagnostics;
    using System;
    using System.Diagnostics;
    //using Plisky.Test;
    using System.Threading;
    using Xunit;

    public class ResolverTests {


        [Fact(DisplayName = nameof(SourceLevel_ContstrucorVerboseIncludesSubLevels1))]
        public void SourceLevel_ContstrucorVerboseIncludesSubLevels1() {
            Bilge b = new Bilge(tl: SourceLevels.Verbose);

            Assert.True((b.ActiveTraceLevel & SourceLevels.Verbose) == SourceLevels.Verbose);
            Assert.True((b.ActiveTraceLevel & SourceLevels.Information) == SourceLevels.Information);
            Assert.True((b.ActiveTraceLevel & SourceLevels.Error) == SourceLevels.Error);
        }


        [Fact(DisplayName = nameof(SourceLevel_ContstrucorVerboseIncludesSubLevels2))]
        public void SourceLevel_ContstrucorVerboseIncludesSubLevels2() {
            Bilge b = new Bilge(tl: SourceLevels.Error);

            Assert.False((b.ActiveTraceLevel & SourceLevels.Verbose) == SourceLevels.Verbose);
            Assert.False((b.ActiveTraceLevel & SourceLevels.Information) == SourceLevels.Information);
            Assert.True((b.ActiveTraceLevel & SourceLevels.Error) == SourceLevels.Error);
        }


        [Fact(DisplayName = nameof(SourceLevel_ContstrucorVerboseIncludesSubLevels3))]
        public void SourceLevel_ContstrucorVerboseIncludesSubLevels3() {
            Bilge b = new Bilge(tl: SourceLevels.Information);

            Assert.False((b.ActiveTraceLevel & SourceLevels.Verbose) == SourceLevels.Verbose);
            Assert.True((b.ActiveTraceLevel & SourceLevels.Information) == SourceLevels.Information);
            Assert.True((b.ActiveTraceLevel & SourceLevels.Error) == SourceLevels.Error);
        }

        [Fact(DisplayName = nameof(Resolver_Exploratory1))]
        [Trait("age","fresh")]
        public void Resolver_Exploratory1() {
            Bilge b = new Bilge();            
            Assert.True(b.CurrentTraceLevel == TraceLevel.Off);
        }


        [Fact(DisplayName = nameof(Resolver_GetsName))]
        public void Resolver_GetsName() {
            const string INAME = "InstanceName";           
            Bilge.SetConfigurationResolver((nm, def) => {
                Assert.Equal(INAME, nm);
                return SourceLevels.Off;
            });
            Bilge b = new Bilge(INAME);
        }

        [Fact(DisplayName = nameof(Resolver_GetsInitialValue))]
        public void Resolver_GetsInitialValue() {
            const string INAME = "InstanceName";
           
            Bilge.SetConfigurationResolver((nm, def) => {
                Assert.Equal<SourceLevels>(SourceLevels.Critical, def);
                return SourceLevels.Off;
            });

            Bilge b = new Bilge(INAME, tl: SourceLevels.Critical);
        }


        [Fact(DisplayName = nameof(Resolver_OverwritesConstructor))]        
        public void Resolver_OverwritesConstructor() {
           
            Bilge.SetConfigurationResolver((nm, def) => {
                return SourceLevels.Verbose;
            });
            Bilge b = new Bilge();

            Assert.Equal(SourceLevels.Verbose, b.ActiveTraceLevel);
        }


    }

}