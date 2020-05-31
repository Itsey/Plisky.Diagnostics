
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
    using System.Text;
    using System.Threading;

    public partial class BilgeWriter  {

#if NET452 || NETSTANDARD2_0
        /// <summary>
        /// Dumps an object into the trace stream, using a series of different approaches for displaying the object depending
        /// on the type of the object that is dumped.
        /// </summary>
        /// <param name="target">The object to be displayed in the trace stream</param>
        /// <param name="context">A context string for the trace entry</param>
        public void Dump(object target, string context, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            InternalDump(target, context, null, meth, pth, ln);
        }
#endif


        #region timer public interface
#if NET452  || NETSTANDARD2_0
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
        public void TimeStart(string timerTitle, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {

        #region entry code

            if ((timerTitle == null) || (timerTitle.Length == 0)) { throw new ArgumentNullException("timerTitle", "timerTitle parameter cannot be null or empty for a call to TimeStart"); }

        #endregion
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
        public void TimeStart(string timerTitle, string timerCategoryName,[CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {

        #region entry code

            if ((timerTitle == null) || (timerTitle.Length == 0)) { throw new ArgumentNullException("timerTitle", "timerTitle parameter cannot be null or empty for a call to TimeStart"); }

        #endregion
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
        /// <param name="meth">The Method Name</param>
        /// <param name="pth">The caller path</param>
        /// <param name="ln">The Line Number</param>
        [Conditional("TRACE")]
        public void TimeStop(string timerTitle, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {

        #region entry code

            if ((timerTitle == null) || (timerTitle.Length == 0)) { throw new ArgumentNullException("timerTitle", "The timerTitle cannot be null or empty when calling TimeStop"); }

        #endregion

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
        /// <param name="meth">The Method Name</param>
        /// <param name="pth">The caller path</param>
        /// <param name="ln">The Line Number</param>
        public void TimeStop(string timerTitle, string timerCategoryName, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {

        #region entry code
            if ((timerTitle == null) || (timerTitle.Length == 0)) {
                throw new ArgumentNullException("timerTitle", "The timerTitle cannot be null or empty when calling TimeStop");
            }

        #endregion

            MessageMetadata mmd = new MessageMetadata(meth, pth, ln);
            InternalTimeCheckpoint(mmd,timerTitle, timerCategoryName, false);
        }
#endif





        #endregion

#if NET452 || NETSTANDARD2_0
        /// <summary>
        /// The enter section method marks a section of debugging code into a descreet block.  Sections are marked on a per
        /// thread basis and can be used by viewers or by Tex to alter the trace output.
        /// </summary>
        /// <remarks><para>While it should be possible to disable output by section this is not implemented yet
        /// either in Tex or in any of the shipped viewers , including mex.</para>
        /// <para>This method is dependant on the TRACE preprosessing identifier.</para>
        /// </remarks>
        /// <param name="sectionName">The friendly name of the secion</param>
        /// <param name="meth">The Method Name</param>
        /// <param name="pth">The caller path</param>
        /// <param name="ln">The Line Number</param>
        [Conditional("TRACE")]
        public void EnterSection(string sectionName, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
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
        public void LeaveSection([CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
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
        public void E(string entryContext = null, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {

            if (entryContext == null) { entryContext = string.Empty; }
            MessageMetadata mmd = new MessageMetadata(meth, pth, ln);
            InternalE(mmd,entryContext);
        }

        /// <summary>
        /// The X override is the indicator for leaving a block that has been entered with E.
        /// </summary>
        /// <remarks><para>This method is dependant on the TRACE preprosessing identifier.</para></remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X", Justification = "Maintained name for backward compatibility")]
        public void X(string exitContext = null, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {


            if (exitContext == null) { exitContext = string.Empty; }
            MessageMetadata mmd = new MessageMetadata(meth, pth, ln);
            InternalX(mmd,exitContext);

        }

        public void Log(TraceSwitch ts, string v, string moreInfo = null, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            ActiveRouteMessage(TraceCommandTypes.LogMessage, v, moreInfo, meth, pth, ln);
        }


        public void Log(string v, string moreInfo = null, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            ActiveRouteMessage(TraceCommandTypes.LogMessage, v, moreInfo, meth, pth, ln);
        }

        public void Flow(string moreInfo = null, [CallerMemberName]string meth = null, [CallerFilePath] string pth = null, [CallerLineNumber]int ln = 0) {
            string msg = $"Flow [{meth}]";
            ActiveRouteMessage(TraceCommandTypes.LogMessage, msg, moreInfo, meth, pth, ln);
        }
#endif




    }
}
