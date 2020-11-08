using Plisky.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Plisky.Diagnostics {
    public abstract class BaseMessageFormatter : IMessageFormatter {
        public const string DEFAULT_UQR = "--uqr--";
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
            for (int i = 1; i < result.Length - 1; i++) {
                result[i] = truncateStartIdentifier + result[i] + Constants.MESSAGETRUNCATE;
            }
            result[result.Length - 1] = truncateStartIdentifier + result[result.Length - 1];
            return result;
        }

        protected MessageFormatterOptions mfo;
        
        

        public BaseMessageFormatter() {
            mfo = new MessageFormatterOptions();
        }

        

        protected abstract string ActualConvert(MessageMetadata msg);

        protected virtual string DefaultConvertWithReference(MessageMetadata msg, string uniquenessReference) {
            string result = ActualConvert(msg);
            if (uniquenessReference!=DEFAULT_UQR) {
                result = result.Replace(DEFAULT_UQR, uniquenessReference);
            }
            return result;
        }
            
        

        public string Convert(MessageMetadata msg) {
            return ConvertWithReference(msg, DEFAULT_UQR);
        }

        public string ConvertWithReference(MessageMetadata msg, string uniquenessReference) {
            if (msg == null) {
                throw new ArgumentNullException(nameof(msg), "Can not convert a null message data");
            }
            if (string.IsNullOrEmpty(uniquenessReference)) {
                uniquenessReference = DEFAULT_UQR;
            }
            msg.NullsToEmptyStrings();
            var result = DefaultConvertWithReference(msg, uniquenessReference);
            if ((mfo.AppendNewline)&&(!result.EndsWith(Environment.NewLine))) {
                result = result + Environment.NewLine;
            }
            return result;
        }
    }
}
