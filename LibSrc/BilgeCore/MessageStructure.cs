namespace Plisky.Plumbing {
    using System;
    using System.Text.RegularExpressions;

    // This class is shared across two projects some of which use some parts and not others.
    // #pragma warning disable 0169

    public sealed class TraceMessageFormat {

        #region constructor / destructor

        // stop auto constructor
        private TraceMessageFormat() {
        }

        #endregion

        // Timer message strings
        // two strings - string 1 is for display string 2 is timer data
        // format TMRDATA[#X#TimerInstanceTitle#X#][#X#TimerSinkTitle#X#][#X#TimeStart#X#]

        //timeString = ">TRCTMR<|SRT|" + dt.Day + "|" + dt.Month + "|" + dt.Year + "|" + dt.Hour + "|" + dt.Minute + "|" + dt.Second + "|" + dt.Millisecond + "|";
        //internal static Regex m_timerPartsCache = new Regex(@"\[#X_X#[0-9A-Za-z,&\.:_()\s-|\[\]]{0,}#X_X#\]", RegexOptions.Compiled);

        public static string PackageDateTimeForTexString(DateTime sendme) {
            // have had all sort of issues with date tiem formatting and settings and timezones etc. Gone to this.
            return string.Format("|DTFMT1|{0}|{1}|{2}|{3}|{4}|{5}|{6}|", sendme.Day, sendme.Month, sendme.Year, sendme.Hour, sendme.Minute, sendme.Second, sendme.Millisecond);
        }

        public static DateTime UnpackageDateTimeFromTexString(String packaged) {
            // have had all sort of issues with date tiem formatting and settings and timezones etc. Gone to this.
            if (!packaged.StartsWith("|DTFMT1|")) {
                throw new InvalidOperationException("Not the right type of date format string to work with");
            }

            DateTime result = new DateTime();
            string[] dtParts = packaged.Substring(8).Split('|');
            //                            yy                          mm               dd                      hh                 mins               secs                          milisecs
            result = new DateTime(int.Parse(dtParts[2]), int.Parse(dtParts[1]), int.Parse(dtParts[0]), int.Parse(dtParts[3]), int.Parse(dtParts[4]), int.Parse(dtParts[5]), int.Parse(dtParts[6]));
            return result;
        }

        public static string AssembleTimerContentString(string timerInstancetitle, string timerSinkTitle, DateTime timeValue) {
            string cd = Constants.TIMER_STRINGENDDELIMITER + Constants.TIMER_STRINGSTARTDELIMITER;
            string timeRepresentation = PackageDateTimeForTexString(timeValue);
            string result = Constants.TIMER_STRINGSTARTCONTENTSTRING + timerInstancetitle + cd + timerSinkTitle + cd + timeRepresentation + Constants.TIMER_STRINGENDDELIMITER;

            return result;
        }

        public static bool SplitTimerStringByParts(string timerString, out string timerInstanceTitle, out string timerSinkTitle, out DateTime timeValue) {
            timerInstanceTitle = timerSinkTitle = null; timeValue = DateTime.MinValue;

            if (!timerString.StartsWith(Constants.TIMER_STRINGSTARTCONTENTSTRING)) { return false; }

            string tempForDatetime;

            Regex m_timerPartsCache = new Regex(@"\[#X_X#[0-9A-Za-z,&\.;\^£:_()<>`\s-|\[\]]{0,}#X_X#\]", RegexOptions.Compiled);

            Match m = m_timerPartsCache.Match(timerString);
            // This should return 5 matches for a legit debug string

            int lengthOfSurrounds = Constants.TIMER_STRINGSTARTDELIMITER.Length + Constants.TIMER_STRINGENDDELIMITER.Length;
            int offsetIntoString = Constants.TIMER_STRINGSTARTDELIMITER.Length;

            timerInstanceTitle = m.Captures[0].Value;
            timerInstanceTitle = timerInstanceTitle.Substring(offsetIntoString, timerInstanceTitle.Length - lengthOfSurrounds);

            m = m.NextMatch();
            timerSinkTitle = m.Captures[0].Value;
            timerSinkTitle = timerSinkTitle.Substring(offsetIntoString, timerSinkTitle.Length - lengthOfSurrounds);

            m = m.NextMatch();
            tempForDatetime = m.Captures[0].Value;
            tempForDatetime = tempForDatetime.Substring(offsetIntoString, tempForDatetime.Length - lengthOfSurrounds);
            timeValue = UnpackageDateTimeFromTexString(tempForDatetime);

            return true;
        }

        public static string AssembleResourceContentString(string name, string context, string valueStr) {
            string result = string.Format(Constants.FORMATTEDRESOURCESTRING, name, valueStr, context);

#if DEBUG
            // Make sure the assemble and split represent one another.

            if (!SplitResourceContentStringByParts(result, out string tname, out string tcontext, out string tvalue)) {
                throw new InvalidOperationException("The splitting of a resource string that has just been assembled did not produce consistant results");
            }

            if (tname != name) {
                throw new InvalidOperationException("The splitting of a resource string that has just been assembled messed up the resource name");
            }
            if (tcontext != context) {
                throw new InvalidOperationException("The splitting of a resource string that has just been assembled messed up the context");
            }
            if (tvalue != valueStr) {
                throw new InvalidOperationException("The splitting of a resource string that has just been assembled messed up the value");
            }

#endif
            return result;
        }

        public static bool SplitResourceContentStringByParts(string packedString, out string resourceName, out string resourceIdent, out string resourceValueRepresentation) {
            resourceName = resourceIdent = resourceValueRepresentation = null;

            if (!packedString.StartsWith(Constants.RESPACKSTRINGIDENT)) {
                return false;
            }

            // Looks valid ish.  Time to split the string into its resorces parts.

            // Chop the initial marker and the first NAME delimiter tag off the string.
            // String goes from MARKER DELIMITER1 NAME DELIMITER1 DELIMITER2 VALUE DELMITER2 DELIMITER3 CONTEXT DELIMITER 3 to
            //                                    NAME DELIMITER1 DELIMITER2 VALUE DELMITER2 DELIMITER3 CONTEXT DELIMITER 3
            int offset = Constants.RESPACKSTRINGIDENT.Length + Constants.RESNAMEDELIMITER.Length;
            string temp = packedString.Substring(offset);

            // Now copy up to the name / value delimiter pair
            offset = temp.IndexOf(Constants.RESNAMEDELIMITER + Constants.RESVALUEDELIMITER);
            resourceName = temp.Substring(0, offset);
            offset += Constants.RESNAMEDELIMITER.Length + Constants.RESVALUEDELIMITER.Length;

            // String goes from NAME DELIMITER1 DELIMITER2 VALUE DELMITER2 DELIMITER3 CONTEXT DELIMITER 3 to
            //                  VALUE DELMITER2 DELIMITER3 CONTEXT DELIMITER 3
            temp = temp.Substring(offset);

            offset = temp.IndexOf(Constants.RESVALUEDELIMITER + Constants.RESCONTEXTDELIMITER);
            resourceValueRepresentation = temp.Substring(0, offset);
            offset += Constants.RESVALUEDELIMITER.Length + Constants.RESCONTEXTDELIMITER.Length;

            // String goes from NAME DELIMITER1 DELIMITER2 VALUE DELMITER2 DELIMITER3 CONTEXT DELIMITER 3 to
            //                  CONTEXT DELIMITER 3
            temp = temp.Substring(offset);

            offset = temp.IndexOf(Constants.RESCONTEXTDELIMITER);
            resourceIdent = temp.Substring(0, offset);

            return true;
        }

        // This file holds the definitions for both Tex and Mex about how the strings that are sent around look.  Mex is defined with MEXBUILD and tex
        // is defined with TEXBUILD so that they can share the same file without interrupting each others build.

        /// <summary>
        /// This regex will match if the string that is tested against it is a fully qualified and valid Tex String, it will attempt to fail any non valid tex
        /// strings.
        /// </summary>
        // todo: make sure it only supports known tex types in between ##
        // TODO: This is a buggy regex, it matches too well - even non numeric PIDs are matched and they shouldnt be.
        public const string IS_VALID_TEXTSTRING_REGEX = @"\{\[[0-9A-Za-z\._]{0,}\]\[[0-9]{1,}\]\[[0-9]{1,}\]\[[0-9A-Za-z\._]{0,}\]\[[0-9A-Za-z\._]{0,}\]\[[0-9]{0,8}\]\[[0-9A-Za-z\.\:_<>`]{0,}\]\}\#[A-Z]{3,3}\#";

        private static Regex s_cachedTexRegex;

        /// <summary>
        /// This method will check the passed string to see if the string passed is not a tex string, it is optimised to fail as soon as possible therefore only guarantees
        /// that the string passed does not look like a tex string
        /// </summary>
        /// <param name="theString">The string to verify whether its tex compatible or not</param>
        public static bool IsTexString(string theString) {
            if ((theString == null) || (theString.Length == 0)) { return false; }
            if (!theString.StartsWith("{[")) { return false; }

            if (s_cachedTexRegex == null) {
                s_cachedTexRegex = new Regex(IS_VALID_TEXTSTRING_REGEX, RegexOptions.Compiled);
            }

            return s_cachedTexRegex.IsMatch(theString);
        }

        // Added support for older strings so can use the new viewer with older data"
        // NB Legacy does not support 64 bit apps, pids and proc ids limited to 5 digit
        public const string SUPPORTED_TEX_LEGACYREGEX = @"\{\[[0-9A-Za-z\._]{0,}\]\[[0-9]{1,5}\]\[[0-9]{1,5}\]\[[0-9A-Za-z\.]{0,}\]\[[0-9]{0,8}\]\}\#[A-Z]{3,3}\#";

        private static Regex s_cachedTexLegacyRegex;

        public static bool IsLegacyTexString(string theString) {
            if ((theString == null) || (theString.Length == 0)) { return false; }
            if (!theString.StartsWith("{[")) { return false; }

            if (s_cachedTexLegacyRegex == null) {
                s_cachedTexLegacyRegex = new Regex(SUPPORTED_TEX_LEGACYREGEX, RegexOptions.Compiled);
            }

            return s_cachedTexLegacyRegex.IsMatch(theString);
        }

        /// <summary>
        /// This identifys the internal structure of the trace message.  The helper functions here
        /// depend on this internal structure and are all kept here.  There should be no dependance
        /// on the structure of the string outside of thsi class.
        ///
        /// {[MACHINENAME][PROCESSID][THREADID][MODULENAME][LINENUMBER][MOREDATA]}#CMD#TEXTOFDEBUGSTRING
        ///
        /// Where MACHINENAME = Current machine name taken from Environment
        /// Where PROCESSID   = The PID assigned to the process that outputed the string
        /// where THREADID    = The numeric ID assigned to the OS Thread running the commands
        /// where MODULENAME  = The cs filename that was executing the commands
        /// where LINENUMBER  = the numeric line number that the debug string was written from
        /// NB Future enhancement 1 :
        ///
        /// </summary>
        public static void ReturnPartsOfStringLegacy(string debugString, out string cmdType, out string ProcessID, out string MachineName, out string ThreadID,
          out string ModuleName, out string LineNumber, out string debugOutput) {
            ProcessID = null; MachineName = null; ThreadID = null; debugOutput = null;

            if (s_returnPartsRegexCache == null) {
                s_returnPartsRegexCache = new Regex(@"\[[0-9A-Za-z\.:_]{0,}\]", RegexOptions.Compiled);
            }

            Match m = s_returnPartsRegexCache.Match(debugString);
            // This should return 5 matches for a legit debug string

            // TODO I believe this deletes string names where [] is passed as initial or final chars test and fix.

            // Get each of the location identifiers from the string. - removing the surrouding
            // [] delimiters from each of the values.
            MachineName = m.Captures[0].Value.Trim(new char[] { '[', ']' });
            m = m.NextMatch();
            ProcessID = m.Captures[0].Value.Trim(new char[] { '[', ']' });
            m = m.NextMatch();
            ThreadID = m.Captures[0].Value.Trim(new char[] { '[', ']' });
            m = m.NextMatch();
            ModuleName = m.Captures[0].Value.Trim(new char[] { '[', ']' });
            m = m.NextMatch();
            LineNumber = m.Captures[0].Value.Trim(new char[] { '[', ']' });

            // Now get the command type and turn it into an enum
            Match cmdMatch = Regex.Match(debugString, "#[A-Z]{3,3}#");
            cmdType = cmdMatch.Captures[0].Value;

            // finally get the rest of the string as the debug message
            debugOutput = debugString.Substring(cmdMatch.Index + Constants.COMMANDSTRINGLENGTH);  // commandstrlen currently 5
        }

        public static string AssembleTexStringFromPartsStructure(MessageParts mp) {
            string result;

            if (mp.SecondaryMessage.Length > 0) {
                result = "{[" + mp.MachineName + "][" + mp.ProcessId + "][" + mp.osThreadId + "][" + mp.netThreadId + "][" + mp.ModuleName + "][" + mp.lineNumber + "][" + mp.AdditionalLocationData + "]}" + mp.MessageType + mp.DebugMessage + Constants.SECONDARYSTRINGSEPARATOR + mp.SecondaryMessage;
            } else {
                result = "{[" + mp.MachineName + "][" + mp.ProcessId + "][" + mp.osThreadId + "][" + mp.netThreadId + "][" + mp.ModuleName + "][" + mp.lineNumber + "][" + mp.AdditionalLocationData + "]}" + mp.MessageType + mp.DebugMessage;
            }
            return result;
        }

        private static Regex s_returnPartsRegexCache;

        /// <summary>
        /// This identifys the internal structure of the trace message.  The helper functions here
        /// depend on this internal structure and are all kept here.  There should be no dependance
        /// on the structure of the string outside of thsi class.
        ///
        /// {[MACHINENAME][PROCESSID][THREADID][NETTHREADID][MODULENAME][LINENUMBER][MOREDATA]}#CMD#TEXTOFDEBUGSTRING
        ///
        /// Where MACHINENAME = Current machine name taken from Environment
        /// Where PROCESSID   = The PID assigned to the process that outputed the string
        /// where THREADID    = The numeric ID assigned to the OS Thread running the commands
        /// where NETTHREADID = The name of the .net thread running the command.
        /// where MODULENAME  = The cs filename that was executing the commands
        /// where LINENUMBER  = the numeric line number that the debug string was written from
        /// where MOREDATA    = this is additional location data, using the form Class::Method when called within Tex.
        /// NB Future enhancement 1 :
        ///
        /// </summary>
        public static void ReturnPartsOfString(string debugString, out string cmdType, out string ProcessID, out string netThreadId, out string MachineName, out string ThreadID,
          out string ModuleName, out string LineNumber, out string moreLocInfo, out string debugOutput) {
            ProcessID = null; MachineName = null; ThreadID = null; debugOutput = null; moreLocInfo = null; netThreadId = null;

            if (s_returnPartsRegexCache == null) {
                s_returnPartsRegexCache = new Regex(@"\[[0-9A-Za-z\.:<>`_]{0,}\]", RegexOptions.Compiled);
            }

            Match m = s_returnPartsRegexCache.Match(debugString);
            // This should return 5 matches for a legit debug string

            // Get each of the location identifiers from the string. - removing the surrouding
            // [] delimiters from each of the values.
            MachineName = m.Captures[0].Value.Trim(new char[] { '[', ']' });
            m = m.NextMatch();
            ProcessID = m.Captures[0].Value.Trim(new char[] { '[', ']' });
            m = m.NextMatch();
            ThreadID = m.Captures[0].Value.Trim(new char[] { '[', ']' });
            m = m.NextMatch();
            netThreadId = m.Captures[0].Value.Trim(new char[] { '[', ']' });
            m = m.NextMatch();
            ModuleName = m.Captures[0].Value.Trim(new char[] { '[', ']' });
            m = m.NextMatch();
            LineNumber = m.Captures[0].Value.Trim(new char[] { '[', ']' });
            m = m.NextMatch();
            moreLocInfo = m.Captures[0].Value.Trim(new char[] { '[', ']' });

            // Now get the command type and turn it into an enum
            Match cmdMatch = Regex.Match(debugString, "#[A-Z]{3,3}#");
            cmdType = cmdMatch.Captures[0].Value;

            // finally get the rest of the string as the debug message
            debugOutput = debugString.Substring(cmdMatch.Index + Constants.COMMANDSTRINGLENGTH);  // commandstrlen currently 5
        }

        public static bool IsTraceCommand(TraceCommandTypes tct) {
            return (((uint)tct & TRACECOMMANDS) == (uint)tct);
        }

        public static bool IsResourceCommand(TraceCommandTypes tct) {
            return (((uint)tct & RESOURCECOMMANDS) == (uint)tct);
        }

        public static bool IsExceptionCommand(TraceCommandTypes tct) {
            return (((uint)tct & EXCEPTIONCOMMANDS) == (uint)tct);
        }

        public static bool IsLogMessageCommand(TraceCommandTypes tct) {
            return (((uint)tct & LOGCOMMANDS) == (uint)tct);
        }

        public static bool IsSectionCommand(TraceCommandTypes tct) {
            return (((uint)tct & SECTIONCOMMANDS) == (uint)tct);
        }

        public const uint TRACECOMMANDS = 0x00000070;
        public const uint RESOURCECOMMANDS = 0x001C0000;
        public const uint EXCEPTIONCOMMANDS = 0x0000F000;
        public const uint LOGCOMMANDS = 0x00000007;
        public const uint SECTIONCOMMANDS = 0x00030000;
    }

    // #pragma warning restore 0169
}