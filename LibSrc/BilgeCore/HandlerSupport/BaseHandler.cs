namespace Plisky.Diagnostics.Listeners {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;


    public abstract class BaseHandler {
        public int Priority { get; set; }
        public IMessageFormatter Formatter { get; protected set; }

        protected IMessageFormatter DefaultFormatter(bool interactive) {
            if (interactive) {
                return new PrettyReadableFormatter();
            }
            return new LegacyFlimFlamFormatter();
        }

        public virtual void Flush() {
        }

        public virtual void CleanUpResources() {
        }

        

        public void SetFormatter(IMessageFormatter fmt) {
            if (fmt != null) {
                Formatter = fmt;
            }
        }

        public BaseHandler() {
            Priority = 5;
            Formatter = DefaultFormatter(false);
        }

    }

}