
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Plisky.Diagnostics {
    public class BilgeUtil : BilgeRoutedBase {

        public void Online(string processName) {

            var asm = Assembly.GetEntryAssembly();
            if (asm==null) {
                asm = Assembly.GetCallingAssembly();
            }
            string exeName = "default";
            if (asm!=null) {
                exeName = asm.GetName().Name;
            }
            
            string msg = $"{Constants.DATAINDICATOR}{Constants.PROCNAMEIDENT_PREFIX}{exeName}{Constants.PROCNAMEIDENT_POSTFIX}";
            ActiveRouteMessage(TraceCommandTypes.MoreInfo, msg, processName);

            msg = $"{Constants.DATAINDICATOR}W{asm.EscapedCodeBase}";
            ActiveRouteMessage(TraceCommandTypes.MoreInfo, msg);
        
        }
        public BilgeUtil(BilgeRouter rt, ConfigSettings cs) : base(rt, cs) {
        }
    }
}
