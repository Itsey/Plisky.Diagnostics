﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Plisky.Diagnostics.Test {
    public class TestHelper {
        public static Bilge GetBilge(string context = null, bool setTrace = true) {
            Bilge result;
            if (context != null) {
                result = new Bilge(context, resetDefaults: true);
            } else {
                result = new Bilge(resetDefaults: true);
            }

            Assert.True(result.IsCleanInitialise(), "Unclean!");
            if (setTrace) {
                result.CurrentTraceLevel = System.Diagnostics.TraceLevel.Info;
            }
            return result;
        }

    }
}
