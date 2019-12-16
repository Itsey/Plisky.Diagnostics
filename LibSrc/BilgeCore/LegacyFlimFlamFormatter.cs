namespace Plisky.Diagnostics.Listeners {
    using Plisky.Plumbing;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class LegacyFlimFlamFormatter : IMessageFormatter {
        private static string truncationCache = Constants.MESSAGETRUNCATE + "[" + Environment.MachineName + "][{0}" + Constants.TRUNCATE_DATAENDMARKER;

        

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

            string truncJoinIdentifier = Thread.CurrentThread.GetHashCode().ToString();
            string truncateStartIdentifier = string.Format(truncationCache, truncJoinIdentifier);
            result[0] = result[0] + Constants.MESSAGETRUNCATE + truncJoinIdentifier + Constants.MESSAGETRUNCATE;
            for (int i=1; i < result.Length-1; i++) {
                result[i] = truncateStartIdentifier + result[i] + Constants.MESSAGETRUNCATE;
            }
            result[result.Length - 1] = truncateStartIdentifier + result[result.Length - 1];
            return result;
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