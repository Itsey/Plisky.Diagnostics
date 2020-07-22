using Plisky.Diagnostics.Copy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Plisky.Diagnostics.Test {
    public class ConfigurationTests {



        [Fact(DisplayName = nameof(BasicConfiguration_StartsEmpty))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void BasicConfiguration_StartsEmpty() {

            BilgeConfiguration bc = new BilgeConfiguration();

            Assert.Empty(bc.HandlerStrings);
            Assert.Empty(bc.ResolverMatches);
            Assert.Equal(SourceLevels.Off, bc.OverallSourceLevel);
        }


        [Fact(DisplayName = nameof(ApplyHandler_InvalidString_Blows))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void ApplyHandler_InvalidString_Blows() {
            BilgeConfiguration bc = new BilgeConfiguration();
            Assert.Throws<InvalidOperationException>(() => {
                bc.AddHandlerString("XXX");
            });            
        }


        [Theory(DisplayName = nameof(ValidStringsAreAdded))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        [InlineData("[FILE:C:\\filename.txt]")]
        [InlineData("[TCPR:10.127.55.96:9060]")]
        [InlineData("[OUDS]")]
        [InlineData("[HTTP:http://124.5.6.7/test/out]")]
        public void ValidStringsAreAdded(string stringValue) {

            BilgeConfiguration bc = new BilgeConfiguration();
            bc.AddHandlerString(stringValue);

            Assert.Single(bc.HandlerStrings);
        }


    }
}
