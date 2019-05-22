
namespace Plisky.Diagnostics {
    using Plisky.Plumbing;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    public partial class BilgeWriter {

        #if NET40ONLY
        private bool? cacheFileIOPermissionResult;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal bool AllowedFileIOPermission() {

            if (cacheFileIOPermissionResult == null) {
                // If the cache is not yet valid or we are not using it then we populate it.

                try {
                    FileIOPermission perm = new FileIOPermission(PermissionState.None);
                    perm.AllLocalFiles = (FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery);
                    perm.Demand();
                    cacheFileIOPermissionResult = true;
                } catch (SecurityException) {
                    // We are unable to parse stackwalks for file information.
                    InternalUtil.LogInternalError(InternalUtil.InternalErrorCodes.AccessDeniedToSomething, "FileIOPermission denied prior to calling a stackwalk.", TraceLevel.Verbose);
                    cacheFileIOPermissionResult = false;
                }
            }
            return (bool)cacheFileIOPermissionResult;

        }
        /// <summary><para>
        /// Several routines within the trace implementation require that the line and module and method information are
        /// generated for the statement that they are on.  This routine generates a stack trace from its current location
        /// then walks back up the trace until it finds the first stack frame that is not related to the trace library.  
        /// this frame is the frame that the calling code was and this information is then returned in the ref parameters.</para>
        /// </summary>
        /// <param name="className">The classname where the code was called from</param>
        /// <param name="methodName">The method name where the code was called from</param>
        /// <param name="fileName">The filename containing the code where this was called from</param>
        /// <param name="lineNumber">The line number that the calling line of code was on relative to the filename</param>
        /// <param name="parameters">The parameter information</param>
        /// <param name="getParameters">Determines whether or not extra work should be done to populate the parameters field.  If this is false parameters returns an empty string</param>
        internal void GetStackInfoBeforeTraceClass(out string className, out string methodName, out string fileName, out string lineNumber, out string parameters, bool getParameters) {

            /* This method is called a lot for all of the stack information that is written.  It is surprisingly slow and is the single biggest
               overhead in all of the logging.  During Tex 2.0.x.x there were methods which hard coded starting at Stacktrace(4) but that made
               the code significantly more complex and added only a tiny performance gain.  Therefore I reverted this code back to its current
               state. */

            parameters = string.Empty;
            lineNumber = string.Empty;
            fileName = string.Empty;
            methodName = string.Empty;
            className = string.Empty;


            bool queryFileInformation = AllowedFileIOPermission(); ;

            // We skip the first two frames as we know there are always at least two Tex methods above this call.
            StackTrace currentStack = new StackTrace(2, queryFileInformation);

            for (int frameIdx = 0; frameIdx < currentStack.FrameCount; frameIdx++) {  // go through the stack frames
                StackFrame nextFrame = currentStack.GetFrame(frameIdx);

                MethodBase theMethod = nextFrame.GetMethod();
                className = InternalUtil.GetClassNameFromMethodbase(theMethod);

                if (theMethod.ReflectedType.Assembly == InternalUtil.TraceAssemblyCache) {
                    // skip anything in this assembly.
                    continue;
                }

                methodName = theMethod.Name;

                if (queryFileInformation) {
                    lineNumber = nextFrame.GetFileLineNumber().ToString();
                    fileName = Path.GetFileName(nextFrame.GetFileName());
                } else {
                    fileName = string.Empty;
                    lineNumber = "0";
                }

                if (getParameters) {
                    // Retested this a couple of times, the stringbuilder is consistantly slower.
                    ParameterInfo[] prams = theMethod.GetParameters();
                    int parameterIndex = 0;
                    parameters = "( ";
                    while (parameterIndex < prams.Length) {
                        parameters += prams[parameterIndex].ParameterType.Name + " " + prams[parameterIndex].Name;
                        parameterIndex++;
                        if (parameterIndex != prams.Length) { parameters += ", "; }
                    }
                    parameters += " )";
                }
                return;
            }
        }

        internal string GetMethodDetailsAndLocationDetails(out string moduleName, out string lineNumber) {
            string mth; string cls; string prms;
            GetStackInfoBeforeTraceClass(out cls, out mth, out moduleName, out lineNumber, out prms, false);
            return cls + "::" + mth;
        }

        internal string GetMethodDetailsAndLocationDetails(out string moduleName, out string lineNumber, out string clsname, out string mthname) {
            string prms;
            GetStackInfoBeforeTraceClass(out clsname, out mthname, out moduleName, out lineNumber, out prms, false);
            return clsname + "::" + mthname;
        }

        internal string GetMethodDetailsAndLocationDetails(out string moduleName, out string lineNumber, out string clsname, out string mthname, out string prms) {
            GetStackInfoBeforeTraceClass(out clsname, out mthname, out moduleName, out lineNumber, out prms, true);
            return clsname + "::" + mthname;
        }


        /// <summary>
        /// Dumps an object into the trace stream, using a series of different approaches for displaying the object depending
        /// on the type of the object that is dumped.
        /// </summary>
        /// <param name="target">The object to be displayed in the trace stream</param>
        /// <param name="context">A context string for the trace entry</param>
        public void Dump(object target, string context) {

            GetMethodDetailsAndLocationDetails(out string pth, out string ln, out string _, out string meth);
            if (!int.TryParse(ln, out int lineNo)) {
                lineNo = 0;
            }
            InternalDump(target, context, null, meth, pth, lineNo);
        }



        #region timer public interface

        /// <summary>
        /// <para> TimeStart is used for rudementary timing of sections of code.  Time start will write a time start identifier to the
        /// trace stream and start an internal timer.  When TimeStop is called for the same timer title then the value of the elapsed
        /// time is written to the trace stream.</para><para>
        /// The TimeStart method relies on a unique timerTitle to be passed to it.  There can only be one active timerTitle of the same
        /// name at any one time.  Each TimeStart(timerTitle) method call must be matched with a TimeStop(timerTitle) method call to ensure
        /// that the timing information is writtten to the trace stream.  timerTitles are case sensitive and must be specified exactly.
        /// </para><para>
        /// This is not a highly effective or accurate profilling mechanism but will suffice for quick timings.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>This method is dependant on the DEBUG preprosessing identifier.</para>
        /// <para>This method has a Trace level of Verbose.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when timerTitle is null or a zero length string.</exception>
        /// <param name="timerTitle">The unique title for the timer that is being started.</param>
        [Conditional("DEBUG")]
        public void TimeStart(string timerTitle) {

        #region entry code

            if ((timerTitle == null) || (timerTitle.Length == 0)) { throw new ArgumentNullException("timerTitle", "timerTitle parameter cannot be null or empty for a call to TimeStart"); }

            #endregion

            GetMethodDetailsAndLocationDetails(out string pth, out string line, out string _, out string meth);
            if (!int.TryParse(line, out int ln)) {
                ln = 0;
            }

            MessageMetadata mmd = new MessageMetadata(meth, pth, ln);
            InternalTimeCheckpoint(mmd, timerTitle, Constants.TIMERNAME, true);
        }

        /// <summary>
        /// <para> TimeStart is used for rudementary timing of sections of code.  Time start will write a time start identifier to the
        /// trace stream and start an internal timer.  When TimeStop is called for the same timer title then the value of the elapsed
        /// time is written to the trace stream.</para><para>
        /// The TimeStart method relies on a unique timerTitle to be passed to it.  There can only be one active timerTitle of the same
        /// name at any one time.  Each TimeStart(timerTitle) method call must be matched with a TimeStop(timerTitle) method call to ensure
        /// that the timing information is writtten to the trace stream.  timerTitles are case sensitive and must be specified exactly.
        /// </para><para>
        /// This is not a highly effective or accurate profilling mechanism but will suffice for quick timings.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>This method is dependant on the DEBUG preprosessing identifier.</para>
        /// <para>This method has a Trace level of Verbose.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when timerTitle is null or a zero length string.</exception>
        /// <param name="timerTitle">The unique title for the timer that is being started.</param>
        /// <param name="timerCategoryName">A category describing a collection of related timings.</param>
        [Conditional("DEBUG")]
        public void TimeStart(string timerTitle, string timerCategoryName) {

        #region entry code

            if ((timerTitle == null) || (timerTitle.Length == 0)) { throw new ArgumentNullException("timerTitle", "timerTitle parameter cannot be null or empty for a call to TimeStart"); }

            #endregion

            GetMethodDetailsAndLocationDetails(out string pth, out string line, out string _, out string meth);
            if (!int.TryParse(line, out int ln)) {
                ln = 0;
            }


            MessageMetadata mmd = new MessageMetadata( meth, pth, ln);
            InternalTimeCheckpoint(mmd,timerTitle, timerCategoryName, true);
        }

        /// <summary>
        /// <para> TimeStop will take a corresponding TimeStart entry and record the difference in milliseconds between the TimeStart and
        /// TimeStop method calls.  The results of this along with the start and stop times will then be written to the debugging stream.</para>
        /// <para> The TimeStop method requires that it is called with a timerTitle parameter that matches exactly a timerTitle that has
        /// already been passed to a TimeStart method call. timerTitles are case sensitive and must be specified exactly.
        /// </para><para>
        /// This is not a highly effective or accurate profilling mechanism but will suffice for quick timings.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>This method is dependant on the TRACE preprosessing identifier.</para>
        /// <para>This method has a Trace level of Verbose.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when timerTitle is null or a zero length string.</exception>
        /// <param name="timerTitle">The unique title for the timer that is being started.</param>
        [Conditional("TRACE")]
        public void TimeStop(string timerTitle) {

        #region entry code

            if ((timerTitle == null) || (timerTitle.Length == 0)) { throw new ArgumentNullException("timerTitle", "The timerTitle cannot be null or empty when calling TimeStop"); }

            #endregion

            GetMethodDetailsAndLocationDetails(out string pth, out string line, out string _, out string meth);
            if (!int.TryParse(line, out int ln)) {
                ln = 0;

            }
            MessageMetadata mmd = new MessageMetadata(meth, pth, ln);
            InternalTimeCheckpoint(mmd,timerTitle, Constants.TIMERNAME, false);
        }

        /// <summary>
        /// <para> TimeStop will take a corresponding TimeStart entry and record the difference in milliseconds between the TimeStart and
        /// TimeStop method calls.  The results of this along with the start and stop times will then be written to the debugging stream.</para>
        /// <para> The TimeStop method requires that it is called with a timerTitle parameter that matches exactly a timerTitle that has
        /// already been passed to a TimeStart method call. timerTitles are case sensitive and must be specified exactly.
        /// </para><para>
        /// This is not a highly effective or accurate profilling mechanism but will suffice for quick timings.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>This method is dependant on the TRACE preprosessing identifier.</para>
        /// <para>This method has a Trace level of Verbose.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when timerTitle is null or a zero length string.</exception>
        /// <param name="timerTitle">The unique title for the timer that is being started.</param>
        /// <param name="timerCategoryName">A category describing a collection of related timings.</param>
        public void TimeStop(string timerTitle, string timerCategoryName) {

        #region entry code
            if ((timerTitle == null) || (timerTitle.Length == 0)) {
                throw new ArgumentNullException("timerTitle", "The timerTitle cannot be null or empty when calling TimeStop");
            }

            #endregion
            GetMethodDetailsAndLocationDetails(out string pth, out string line, out string _, out string meth);
            if (!int.TryParse(line, out int ln)) {
                ln = 0;
            }
            MessageMetadata mmd = new MessageMetadata(meth, pth, ln);
            InternalTimeCheckpoint(mmd,timerTitle, timerCategoryName, false);
        }






        #endregion


        /// <summary>
        /// The enter section method marks a section of debugging code into a descreet block.  Sections are marked on a per
        /// thread basis and can be used by viewers or by Tex to alter the trace output.
        /// </summary>
        /// <remarks><para>While it should be possible to disable output by section this is not implemented yet
        /// either in Tex or in any of the shipped viewers , including mex.</para>
        /// <para>This method is dependant on the TRACE preprosessing identifier.</para>
        /// </remarks>
        /// <param name="sectionName">The friendly name of the secion</param>
        [Conditional("TRACE")]
        public void EnterSection(string sectionName) {

            GetMethodDetailsAndLocationDetails(out string pth, out string line, out string _, out string meth);
            if (!int.TryParse(line, out int ln)) {
                ln = 0;
            }

            ActiveRouteMessage(TraceCommandTypes.SectionStart, sectionName, null, meth, pth, ln);
        }

        /// <summary>
        /// The exit section method marks the termination of a section of code.  Section enter and exit blocks are used by viewers
        /// to determine which parts of the code to view at once.
        /// </summary>
        /// <remarks>Disabling output per section not implemented yet
        /// <para>This method is dependant on the TRACE preprosessing identifier.</para>
        /// </remarks>
        [Conditional("TRACE")]
        public void LeaveSection() {
            GetMethodDetailsAndLocationDetails(out string pth, out string line, out string _, out string meth);
            if (!int.TryParse(line, out int ln)) {
                ln = 0;
            }
            ActiveRouteMessage(TraceCommandTypes.SectionEnd, "", null, meth, pth, ln);
        }

        /// <summary>
        /// The E override to provide a string will replace the automatically generated method name with the string that you
        /// provide in the first parameter.
        /// </summary>
        /// <remarks><para>This method is dependant on the TRACE preprosessing identifier.</para></remarks>
        /// <param name="entryContext">The name of the block being entered</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "E", Justification = "Maintained name for backward compatibility")]
        [Conditional("TRACE")]
        public void E(string entryContext = null) {
            GetMethodDetailsAndLocationDetails(out string pth, out string line, out string _, out string meth);
            if (!int.TryParse(line, out int ln)) {
                ln = 0;
            }
            if (entryContext == null) { entryContext = string.Empty; }
            MessageMetadata mmd = new MessageMetadata(meth, pth, ln);
            InternalE(mmd,entryContext);
        }

        /// <summary>
        /// The X override is the indicator for leaving a block that has been entered with E.
        /// </summary>
        /// <remarks><para>This method is dependant on the TRACE preprosessing identifier.</para></remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X", Justification = "Maintained name for backward compatibility")]
        public void X(string exitContext = null) {

            GetMethodDetailsAndLocationDetails(out string pth, out string line, out string _, out string meth);
            if (!int.TryParse(line, out int ln)) {
                ln = 0;
            }

            if (exitContext == null) { exitContext = string.Empty; }
            MessageMetadata mmd = new MessageMetadata(meth, pth, ln);
            InternalX(mmd,exitContext);

        }

        public void Log(TraceSwitch ts, string v, string moreInfo = null) {
            GetMethodDetailsAndLocationDetails(out string pth, out string line, out string _, out string meth);
            if (!int.TryParse(line, out int ln)) {
                ln = 0;
            }
            ActiveRouteMessage(TraceCommandTypes.LogMessage, v, moreInfo, meth, pth, ln);
        }


        public void Log(string v, string moreInfo = null) {
            GetMethodDetailsAndLocationDetails(out string pth, out string line, out string _, out string meth);
            if (!int.TryParse(line, out int ln)) {
                ln = 0;
            }

            ActiveRouteMessage(TraceCommandTypes.LogMessage, v, moreInfo, meth, pth, ln);
        }

        public void Flow(string moreInfo = null) {
            GetMethodDetailsAndLocationDetails(out string pth, out string line, out string _, out string meth);
            if (!int.TryParse(line, out int ln)) {
                ln = 0;
            }

            string msg = $"Flow [{meth}]";
            ActiveRouteMessage(TraceCommandTypes.LogMessage, msg, moreInfo, meth, pth, ln);
        }
#endif




    }
}
