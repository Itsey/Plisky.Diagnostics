using Plisky.Diagnostics.Copy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Plisky.Diagnostics.Test {
    public class AlertFeatureTests {



        [Fact(DisplayName = nameof(BasicAlert_GetsWritten_TraceOff))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void BasicAlert_GetsWritten_TraceOff() {
            Bilge sut = TestHelper.GetBilgeAndClearDown();

            var mmh = new MockMessageHandler();
            sut.AddHandler(mmh);

            Bilge.Alert.Online("test-appname");
            sut.Flush();

            Assert.Equal(1, mmh.TotalMessagesRecieved);
        }

    }
}
