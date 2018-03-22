namespace Plisky.Diagnostics.Listeners {
    using Plisky.Plumbing;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class LegacyFlimFlamFormatter : IMessageFormatter {
        private static string truncationCache = Constants.MESSAGETRUNCATE + "[" + Environment.MachineName + "][{0}" + Constants.TRUNCATE_DATAENDMARKER;

        [SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api", Justification = "Want to send to ODS not to a debugger")]
        [DllImport("kernel32.dll", EntryPoint = "OutputDebugStringA", SetLastError = false)]
        public static extern void OutputDebugString(String s);

        /// <summary>
        /// This will take a single long string and return it as a series of truncated strings with the length that is
        /// specified in theLength parameter used to do the chopping up.  There is nothing clever or special about this
        /// routine it does not break on words or aynthing like that.
        /// </summary>
        /// <param name="theLongString">The string that is to be chopped up into smaller strings</param>
        /// <param name="theLength">The length at which the smaller strings are to be created</param>
        /// <returns></returns>
        public static string[] MakeManyStrings(string theLongString, int theLength) {

            #region entry code

            if (theLongString == null) { return null; }
            if (theLength <= 0) { throw new ArgumentException("theLength parameter cannot be <=0 for MakeManyStrings method"); }

            #endregion

            string[] result;

            if (theLongString.Length <= theLength) {
                // Special case where no splitting is necessary;
                result = new string[1];
                result[0] = theLongString;
                return result;
            }

            double exactNoChops = (double)((double)theLongString.Length / (double)theLength);
            int noChops = (int)Math.Ceiling(exactNoChops);

            result = new string[noChops];

            // All other cases where theLongString actually needs to be chopped up into smaller chunks
            int remainingChars = theLongString.Length;
            int currentOffset = 0;
            int currentChopCount = 0;
            while (remainingChars > theLength) {
                result[currentChopCount++] = theLongString.Substring(currentOffset, theLength);
                remainingChars -= theLength;
                currentOffset += theLength;
            }
            result[currentChopCount] = theLongString.Substring(currentOffset, remainingChars);

#if DEBUG
            if (currentChopCount != (noChops - 1)) {
                throw new NotSupportedException("This really should not happen");
            }
#endif

            return result;
        }

        public static void internalOutputDebugString(string thisMsg) {
            try {
                // The output debug string API seems to be a little strange when it comes to handeling large amounts of
                // data and does not seem to be able to handle long strings properly.  It is likely that it is my code
                // that is at fault but untill I can get to the bottom of it this listener will chop long strings up
                // into chunks and send them as separate chunks of 1024 bytes in each.  It is then the viewers job
                // to reassemble them, however in order to help the viewer specialist string truncated identifiers will
                // be sent as the markers to the extended strings.
                if (thisMsg.Length > Constants.LIMIT_OUTPUT_DATA_TO) {
                    string truncJoinIdentifier = Thread.CurrentThread.GetHashCode().ToString();
                    string[] messageParts = MakeManyStrings(thisMsg, Constants.LIMIT_OUTPUT_DATA_TO);
                    // Truncation identifier is #TNK#[MACHINENAME][TRUNCJOINID]XEX
                    string truncateStartIdentifier = string.Format(truncationCache, truncJoinIdentifier);

                    OutputDebugString(messageParts[0] + Constants.MESSAGETRUNCATE + truncJoinIdentifier + Constants.MESSAGETRUNCATE);
                    for (int partct = 1; partct < messageParts.Length - 1; partct++) {
                        OutputDebugString(truncateStartIdentifier + messageParts[partct] + Constants.MESSAGETRUNCATE); //+truncJoinIdentifier+Constants.MESSAGETRUNCATE);
                    }
                    OutputDebugString(truncateStartIdentifier + messageParts[messageParts.Length - 1]);  // The last one has no endswith

                    return;
                }

                OutputDebugString(thisMsg);
            } catch (Exception ex) {
                InternalUtil.LogInternalError(InternalUtil.InternalErrorCodes.ODSListenerError, "There was an error trying to send data to outputdebugstring. " + ex.Message);
                throw;
            }
        }

        public string ConvertToString(MessageMetadata msg) {
            string result;
            MessageParts nextMsg = new MessageParts();
            msg.NullsToEmptyStrings();
            // These will cause the stream to be written if we are queueingf messages.
            //if ((messageType == Constants.ASSERTIONFAILED) || (theMessage == Constants.EXCEPTIONENDTAG) || (messageType == Constants.ERRORMSG)) {
            //    nextMsg.TriggerRefresh = true;
            //}
            Emergency.Diags.Log("Formatting string");
            try {


                nextMsg.DebugMessage = msg.Body;
                nextMsg.SecondaryMessage = msg.FurtherDetails;
                nextMsg.ClassName = msg.ClassName;
                nextMsg.lineNumber = msg.LineNumber;
                nextMsg.MethodName = msg.MethodName;
                nextMsg.MachineName = msg.MachineName;
                nextMsg.netThreadId = msg.NetThreadId;
                nextMsg.osThreadId = msg.OSThreadId;
                nextMsg.ProcessId = msg.ProcessId;
                //nextMsg.ModuleName = Path.GetFileNameWithoutExtension(msg.FileName);
                nextMsg.ModuleName = msg.FileName;
                nextMsg.AdditionalLocationData = msg.ClassName + "::" + msg.MethodName;

                // Populate Message Type
                nextMsg.MessageType = TraceCommands.TraceCommandToString(msg.CommandType);

                result = TraceMessageFormat.AssembleTexStringFromPartsStructure(nextMsg);
            } catch ( Exception ex) {
                Emergency.Diags.Log("EXX >> "+ex.Message);
                throw;
            }
            Emergency.Diags.Log("REturning legacvy string"+result);
            return result;
        }
    }
}