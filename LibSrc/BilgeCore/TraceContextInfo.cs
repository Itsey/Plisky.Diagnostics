namespace Plisky.Plumbing {

    internal class TraceContextInfo {
        public string MethodName { get; set; }
        public int? LineNumber { get; set; }

        public string Filename { get; set; }

        internal TraceContextInfo(string meth, int? line, string fn) {
            MethodName = meth;
            LineNumber = line;
            Filename = fn;
        }
    }
}