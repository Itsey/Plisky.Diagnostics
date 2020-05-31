
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

    public partial class BilgeWriter : BilgeConditionalRoutedBase {
        
        private enum StreamDumpUptions { Hex, Tex, Workitout }; // TODO : Seriously?

        internal BilgeWriter(BilgeRouter router, ConfigSettings config, SourceLevels yourTraceLevel) :base(router,config,yourTraceLevel) {
        }


        /// <summary>
        /// Dumps a secure string to the trace structure as a plaintext string.  This is usefull to see your secure
        /// strings without causing them to be seen in release code.
        /// </summary>
        /// <remarks>BE CAREFULL, this could render your entire secure string approach useless.</remarks>
        /// <param name="ss">The secure string to dump</param>
        /// <param name="message">The message representing the secure string</param>
        /// <param name="secondaryMessage">More information relating to the dump</param>
        /// <param name="meth">The Method Name</param>
        /// <param name="pth">The caller path</param>
        /// <param name="ln">The Line Number</param>
        private void InternalDumpSecureString(SecureString ss, string message, string secondaryMessage,string meth, string pth, int ln) {

#region entry code

            if (ss == null) {
                ActiveRouteMessage(TraceCommandTypes.LogMessage, "SecureString value:<string was null>", message, meth, pth, ln);
                return;
            }
            if ((message == null) || (message.Length == 0)) { message = Constants.NFI; }

#endregion

            IntPtr ptr = Marshal.SecureStringToBSTR(ss);
            ActiveRouteMessage(TraceCommandTypes.LogMessage, $"SecureString value: {Marshal.PtrToStringUni(ptr)}", message + Environment.NewLine + secondaryMessage);

        }

        /// <summary>
        /// InternalDumpArray is the fundamental point where array dumping happens.  An array will have its contents
        /// walked and those will be written out to the trace stream.
        /// </summary>
        /// <param name="arr">The array to be examined</param>
        /// <param name="limitSearchTo">Stop dumping the array after this many results, if set to -1 the full array will be dumped</param>
        /// <param name="message">A context message describing the dump</param>
        /// <param name="meth">The Method Name</param>
        /// <param name="pth">The caller path</param>
        /// <param name="ln">The Line Number</param>
        private void InternalDumpArray(Array arr, string message, int limitSearchTo, string meth, string pth, int ln) {

#region entry code

            if ((message == null) || (message.Length == 0)) {
                message = Constants.NFI;
            }

            if (arr == null) {
                ActiveRouteMessage(TraceCommandTypes.LogMessage, "DumpArray, Array object was null", message, meth, pth, ln);
                return;
            }

#endregion


            ActiveRouteMessage(TraceCommandTypes.LogMessage, "DumpArray of " + arr.Length.ToString() + " elements ",message, meth, pth, ln);

            int arrayIndex = 0;
            int endIndex = arr.Length;

            if (limitSearchTo > -1) {
                // Check whether they want to abort the search before hitting all elements of the array.
                endIndex = limitSearchTo;
            }

            for (int elementCount = 0; elementCount < endIndex; elementCount++) {
                object o = arr.GetValue(elementCount);    // Get each element from the array.

                if (o == null) { o = "null"; }

                ActiveRouteMessage(TraceCommandTypes.MoreInfo, arrayIndex.ToString() + "  :  " + o.ToString());
                arrayIndex++;
                if (limitSearchTo > -1) {
                    // If they want to stop the search early then allow this top happen
                    if (arrayIndex > limitSearchTo) { break; }
                }
            }

            ActiveRouteMessage(TraceCommandTypes.MoreInfo, "__________  Dump Array Ends  __________");
        }

        /// <summary>
        /// InternalDumpEnumerable is called from the public facing dump methods to do the actual work.
        /// </summary>
        /// <param name="ien">Enumerable object</param>
        /// <param name="message">Associated Context</param>
        /// <param name="meth">The Method Name</param>
        /// <param name="pth">The caller path</param>
        /// <param name="ln">The Line Number</param>
        private void InternalDumpEnumerable(IEnumerable ien, string message, string meth, string pth, int ln) {
            ActiveRouteMessage(TraceCommandTypes.LogMessage, "Dump Enumerable type.", message, meth, pth, ln);
            int count = 0;
            foreach (object o in ien) {
                count++;
                ActiveRouteMessage(TraceCommandTypes.MoreInfo, " " + count.ToString() + " > " + ObjectToString(o));
            }
        }

        /// <summary>
        /// InternalDumpException is called from the public facing dump exception methods to do the actual work.  It will
        /// explore an exception object that is passed to it and dump it to the logging stream.
        /// </summary>
        /// <param name="ex">An exception to be dumped to the logging stream.</param>
        /// <param name="message">A text description of the exception that is being explored</param>
        /// <param name="message2">Any further associated information</param>
        /// <param name="meth">The Method Name</param>
        /// <param name="pth">The caller path</param>
        /// <param name="ln">The Line Number</param>
        private void InternalDumpException(Exception ex, string message, string message2, string meth, string pth, int ln) {

#region entry code

            if ((message == null) || (message.Length == 0)) {
                message = Constants.NFI;
            }

            if (ex == null) {

                ActiveRouteMessage(TraceCommandTypes.InternalMsg,"DumpException, Exception object was null", message, meth, pth, ln);
                return;
            }

#endregion


            ActiveRouteMessage(TraceCommandTypes.ExceptionBlock, message, message2, meth, pth, ln);

            Exception inex = ex;

            // Depth protection is used to cover off the eventuallity where its technically possible to have some of the inner
            int depthProtection = Constants.DEPTHPROTECTION;

            while ((inex != null) && ((depthProtection--) > 0)) {

                ActiveRouteMessage(TraceCommandTypes.ExcStart, inex.Message, inex.GetType().ToString());
                ActiveRouteMessage(TraceCommandTypes.ExceptionData, "H:" + inex.HelpLink, "T:" + inex.TargetSite);
                ActiveRouteMessage(TraceCommandTypes.ExceptionData, "R:" + inex.Source, "S:" + inex.StackTrace);

                if (inex.Data != null) {
                    string secondary;
                    foreach (object o in inex.Data.Keys) {
                        if (inex.Data[o] != null) {
                            secondary = inex.Data[o].ToString();
                        } else {
                            secondary = string.Empty;
                        }

                        ActiveRouteMessage(TraceCommandTypes.ExceptionData, o.ToString(), secondary);
                    }
                }


                ActiveRouteMessage(TraceCommandTypes.ExcEnd, "", "");
               inex = inex.InnerException;
            }

            ActiveRouteMessage(TraceCommandTypes.ExceptionData, Constants.EXCEPTIONENDTAG, "");
        }

        private void InternalDumpObjectLite(object obj, string titleForDump, string secondaryMessage, string meth, string pth, int ln) {
            secondaryMessage = secondaryMessage ?? string.Empty;

            // Non recursive object dump
            if (obj == null) {
                titleForDump = titleForDump ?? string.Empty;
                ActiveRouteMessage(TraceCommandTypes.LogMessage, titleForDump + "ObjectDump of NULL object", secondaryMessage,meth, pth, ln);
                ActiveRouteMessage(TraceCommandTypes.MoreInfo, titleForDump + "ObjectDump End", "");
                return;
            }

            if (titleForDump == null) {
                ActiveRouteMessage(TraceCommandTypes.LogMessage, "ObjectDump of " + obj.ToString(), secondaryMessage, meth, pth, ln);
            } else {
                ActiveRouteMessage(TraceCommandTypes.LogMessage, titleForDump, secondaryMessage + "\r\nObjectDump of " + obj.ToString(), meth, pth, ln);
            }
            string objectData = ObjectToString(obj);

            ActiveRouteMessage(TraceCommandTypes.MoreInfo, objectData);
            ActiveRouteMessage(TraceCommandTypes.MoreInfo, "ObjectDump End");
        }

        /// <summary>
        /// Helper function to convert system type names to their C# equivalents.  Used to format output to a c# style
        /// although generally unnecessary can help flavour output.
        /// </summary>
        /// <param name="typeName">The .net typename of the variable to convert.</param>
        /// <returns>The c# specific typename corresponding to the .net type</returns>
        private string TranslateTypeNamesToCSharp(string typeName) {

#region entry code

            if (string.IsNullOrEmpty(typeName)) {
                return string.Empty;
            }

#endregion

            switch (typeName) {
                // Do not change these out to nameof - that removes the namespace prefix
                case "System.String": { return "string"; }
                case "System.Int32": { return "int"; }
                case "System.Int16": { return "Int16"; }
                case "System.Int64": { return "Int64"; }
                case "System.Double": { return "double"; }
                case "System.Boolean": { return "bool"; }
            }

            return typeName;
        }

        private string ObjectToString(object obj) {
            if ((obj is int) || (obj is string) || (obj is float) || (obj is long) || (obj is uint) || (obj is bool)) {
                return "<dmp><t>" + obj.GetType().Name + "</t><v>" + obj.ToString() + "</v>";
            }

            BindingFlags bflgs = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo[] fields = obj.GetType().GetFields(bflgs);

            StringBuilder result = new StringBuilder();
            result.Append("<dmp><flds>");
            foreach (FieldInfo field in fields) {
                string prettyType = TranslateTypeNamesToCSharp(field.FieldType.ToString());
                string prettyValue;

                if (field.GetValue(obj) == null) {
                    prettyValue = "null";
                } else {
                    prettyValue = field.GetValue(obj).ToString();
                }

                if (prettyType.Equals("string")) { prettyValue = "\"" + prettyValue + "\""; }
                result.Append(String.Format("<fld><t>{0}</t><n>{1}</n><v>{2}</v></fld> \r\n", prettyType, field.Name, prettyValue));
            }
            result.Append("</flds><prps>");
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo pi in props) {
                string prettyType = TranslateTypeNamesToCSharp(pi.PropertyType.ToString());
                string prettyValue;

                if (pi.GetValue(obj, null) == null) {
                    prettyValue = "null";
                } else {
                    prettyValue = pi.GetValue(obj, null).ToString();
                }

                if (prettyType.Equals("string")) { prettyValue = "\"" + prettyValue + "\""; }
                result.Append(String.Format("<prp><t>{0}</t><n>{1}</n><v>{2}</v></prp> \r\n", prettyType, pi.Name, prettyValue));
            }
            result.Append("</prps></dmp>");
            return result.ToString();
        }

        /// <summary>
        /// InternalDumpHashtable is called to write a hashtable out to the debugging stream.  It can be used internally or is called
        /// directly from the public facing DumpHashTable methods. If called internally then internalCall must be true as this will ensure
        /// that all info is output with Constants.MOREINFO, and therefore form part of the calling methods output rather than
        /// a log in its own right.
        /// </summary>
        /// <param name="ht">The hashtable that is to be written to the trace stream</param>
        /// <param name="contextText">A context string describing the hash table</param>
        /// <param name="internalCall">A boolean to flag if method is being called internall i.e. as part of another dump method</param>
        /// <param name="secondaryMessage">Further contextual information</param>
        /// <param name="meth">The Method Name</param>
        /// <param name="pth">The caller path</param>
        /// <param name="ln">The Line Number</param>
        private void InternalDumpHashTable(Hashtable ht, string contextText, string secondaryMessage, bool internalCall, string meth, string pth, int ln) {


            TraceCommandTypes typeOfMessage = internalCall ? TraceCommandTypes.MoreInfo : TraceCommandTypes.LogMessage;

            if (ht == null) {
                ActiveRouteMessage(typeOfMessage, "Hashtable Dump (" + contextText + ") : null", "", meth, pth, ln);
                return;
            }

            secondaryMessage = secondaryMessage ?? string.Empty;


            ActiveRouteMessage(typeOfMessage, "Hashtable Dump (" + contextText + ") : " + ht.ToString(), secondaryMessage + " \r\n Ht Entries : " + ht.Count.ToString(), meth, pth, ln);
            // output each item in the hashtable
            foreach (System.Collections.DictionaryEntry hEntry in ht) {
                string valText;

                if (hEntry.Value is Hashtable) {
                    valText = "Value is Hashtable with " + ((Hashtable)hEntry.Value).Count.ToString() + " elements.";
                } else if (hEntry.Value is Array) {
                    valText = "Value is Array with " + ((Array)hEntry.Value).Length.ToString() + " elements.";
                } else {
                    valText = hEntry.Value.ToString();
                }
                ActiveRouteMessage(TraceCommandTypes.MoreInfo, String.Format("  Key [{0}]  : Value [{1}]  ", hEntry.Key.ToString(), valText), "");
            }


            ActiveRouteMessage(TraceCommandTypes.MoreInfo, "_______ Hashtable Dump End ________", "");
        }

        private void InternalDumpStream(Stream stm, long startingPosition, int charsToDump, StreamDumpUptions sdo, string context, string supportingInformation) {
            if (charsToDump < 0) {
                // Signifies that we should dump the default amount, specified here as 100 chars
                charsToDump = 100;
            }

            switch (sdo) {
                case StreamDumpUptions.Hex:
                    InternalDumpStreamAsHex(stm, startingPosition, charsToDump, context, supportingInformation);
                    break;

                case StreamDumpUptions.Tex:
                    InternalDumpStreamAsText(stm, startingPosition, charsToDump, context, supportingInformation);
                    break;

                case StreamDumpUptions.Workitout:

                    bool ProbablyText = true;

                    int charsToParse = charsToDump > 100 ? 100 : charsToDump;

                    if (charsToParse > stm.Length) { charsToParse = (int)stm.Length; }

                    for (long i = startingPosition; i <= charsToParse; i++) {
                        int nxt = stm.ReadByte();
                        if (nxt > 127) {
                            ProbablyText = false;
                        }
                    }

                    stm.Seek((-1 * charsToParse), SeekOrigin.Current);  // Reset the position

                    if (ProbablyText) {
                        InternalDumpStreamAsText(stm, startingPosition, charsToDump, context, supportingInformation);
                    } else {
                        InternalDumpStreamAsHex(stm, startingPosition, charsToDump, context, supportingInformation);
                    }
                    break;

                default: throw new NotImplementedException("This should never occur, there should be no other options");
            }
        }

        /// <summary>
        /// InternalDumpStreamAsText will read the start of a stream and write out the start to the trace stream assuming that
        /// the input stream is in a text format that will be readable.  The position of the stream will be reset.
        /// </summary>
        /// <param name="stm">the stream to read</param>
        /// <param name="startingPosition">The position within the stream from which to start dumping</param>
        /// <param name="charsToDump">the number of characters to read</param>
        /// <param name="context">A context description for the dump</param>
        /// <param name="supportingInformation">Further contextual information</param>
        private void InternalDumpStreamAsText(Stream stm, long startingPosition, int charsToDump, string context, string supportingInformation) {



            if (charsToDump <= 0) return;

            // Special cases for empty and null streams.
            if (stm == null) {
                ActiveRouteMessage(TraceCommandTypes.LogMessage, "Stream:: The stream is null", "Cannot perform Stream Dump.");
                return;
            }
            if (stm.Length == 0) {
                ActiveRouteMessage(TraceCommandTypes.LogMessage, "Stream:: The stream is Empty", "Cannot perform Stream Dump.");
                return;
            }

            if (charsToDump > stm.Length) { charsToDump = (int)stm.Length; }

            long streamPositionBeforeDumpCalled = stm.Position;
            stm.Position = startingPosition;

            StreamReader sr = new StreamReader(stm);
            char[] theBuffer = new char[charsToDump];
            if (sr.Read(theBuffer, 0, charsToDump) != charsToDump) {
                // There was a problem reading all of the data.
                ActiveRouteMessage(TraceCommandTypes.InternalMsg, "WARNING :: During DumpStreamAsText stream read operation failed to retrive " + charsToDump.ToString() + " characters", "");
            }

            stm.Position = streamPositionBeforeDumpCalled;

            ActiveRouteMessage(TraceCommandTypes.LogMessage, context + " stream dump (text).", "Stream::" + new string(theBuffer));
            ActiveRouteMessage(TraceCommandTypes.MoreInfo, supportingInformation, " Stream Dump Length[" + stm.Length + "]PosFirst[" + streamPositionBeforeDumpCalled.ToString() + "]Type[" + stm.GetType().ToString() + "]");
        }

        /// <summary>
        /// InternalDumpStreamAsHex will read the start of a stream and write out the start to the trace stream assuming that
        /// the input stream is in a binary format.  The stream will be written as a series of bytes which will be placed into
        /// the trace stream as hex representations.  The position of the stream will be reset.
        /// </summary>
        /// <param name="stm">the stream to read</param>
        /// <param name="startingPosition">The position within the stream from which to start dumping</param>
        /// <param name="charsToDump">the number of characters to read</param>
        /// <param name="context">Contextual information for the dump</param>
        /// <param name="supportingInformation">Further contextual information</param>
        private void InternalDumpStreamAsHex(Stream stm, long startingPosition, int charsToDump, string context, string supportingInformation) {

#region entry code


            context = context ?? string.Empty;
            supportingInformation = supportingInformation ?? string.Empty;

#endregion

            // Special case for null or empty streams
            if ((stm == null) || (stm.Length == 0)) {
                string msg;
                if (stm == null) {
                    msg = "The stream is null.";
                } else {
                    msg = "The stream has a length of 0.";
                }
                ActiveRouteMessage(TraceCommandTypes.LogMessage, "Stream:: " + msg, "Cannot perform Stream Dump.");
                return;
            }

            if (charsToDump <= 0) return;
            if (charsToDump > stm.Length) { charsToDump = (int)stm.Length; }

            long currentPos = stm.Position;
            stm.Position = startingPosition;

            StringBuilder sb = new StringBuilder();
            for (; charsToDump > 0; charsToDump--) {
                int next = stm.ReadByte();
                if (next == -1) {
                    ActiveRouteMessage(TraceCommandTypes.InternalMsg, "WARNING :: During DumpStreamAsHex stream read operation failed to retrive next characters", "");
                    break;
                }
                sb.Append(string.Format("{0:x2}  ", next));
            }

            stm.Position = currentPos;
            ActiveRouteMessage(TraceCommandTypes.LogMessage, context + " stream dump (hex).", "Stream::" + sb.ToString());
            ActiveRouteMessage(TraceCommandTypes.MoreInfo, supportingInformation, " Stream Dump Length[" + stm.Length + "]PosFirst[" + currentPos.ToString() + "]Type[" + stm.GetType().ToString() + "]");
        }

        internal void InternalDump(object o, string context, string secondaryMessage, string meth, string pth, int ln) {
            // Firstly we check to see whether or not its somethign that we have a special format for.
            context = context ?? string.Empty; secondaryMessage = secondaryMessage ?? string.Empty;


            if (o is Exception) {
                InternalDumpException((Exception)o, context, secondaryMessage, meth, pth, ln);
                return;
            }
            if (o is Hashtable) {
                InternalDumpHashTable((Hashtable)o, context, secondaryMessage, false, meth, pth, ln);
                return;
            }
            if (o is SecureString) {
                InternalDumpSecureString((SecureString)o, context, secondaryMessage, meth, pth, ln);
                return;
            }
            if (o is Array) {
                // TODO : Convert this to use a furhter info
                ActiveRouteMessage(TraceCommandTypes.MoreInfo, "<th:" + Thread.CurrentThread.GetHashCode() + ">" + secondaryMessage);
                InternalDumpArray((Array)o, context, -1, meth, pth, ln);
                return;
            }

            if (o is List<String>) {
                // TODO: Send this to use a Further Info

                ActiveRouteMessage(TraceCommandTypes.MoreInfo, "<th:" + Thread.CurrentThread.GetHashCode() + ">" + secondaryMessage);
                InternalDumpArray(((List<String>)o).ToArray(), context, -1, meth, pth, ln);
                return;
            }

            if (o is Stream) {
                InternalDumpStream((Stream)o, 0, -1, StreamDumpUptions.Workitout, context, secondaryMessage);
                return;
            }

            if (o is IEnumerable) {
                // TODO: Internal furtherInfo
                //MessageRouter(Constants.MOREINFO, , null, false);
                ActiveRouteMessage(TraceCommandTypes.MoreInfo, "<th:" + Thread.CurrentThread.GetHashCode() + ">" + secondaryMessage);
                InternalDumpEnumerable((IEnumerable)o, context, meth, pth, ln);
                return;
            }
            // If it isnt we default to the basic reflection approach.
            InternalDumpObjectLite(o, context, secondaryMessage, meth, pth, ln);
        }





        /// <summary>
        /// The _E function is a method of orverriding the E default behaviour of specifying the method name as the
        /// entry point to a block of code.  With _E you can specify a block name, however the corresponding _X function
        /// should be used to indicate the end of the block with the same string passed.
        /// This method is only present when the DEBUG contiditional is specified
        /// </summary>
        /// <param name="theMessage">The textual message to enter with</param>
        internal void InternalE(MessageMetadata mmd, string theMessage) {
            string timeString = null;

            if (sets.AddTimingsToEnterExit) {
                DateTime dt = DateTime.Now;
                timeString = ">TRCTMR<|SRT|" + dt.Day + "|" + dt.Month + "|" + dt.Year + "|" + dt.Hour + "|" + dt.Minute + "|" + dt.Second + "|" + dt.Millisecond + "|";
            }

            mmd.CommandType = TraceCommandTypes.TraceMessageIn;
            mmd.Body = "> " + mmd.MethodName + " (" + theMessage + ")";
            mmd.FurtherDetails = timeString;

            var tcmsg = mmd.Clone();
            InternalTimeCheckpoint(tcmsg, Constants.AUTOTIMER_PREFIX + Constants.INTERNAL_METHOD_REPLACE, Constants.INTERNAL_CLASS_REPLACE, true);

            ActiveRouteMessage(mmd);
        }

        /// <summary>
        /// The _X method is an override for the X function that allowes the specification of the name of the exit block
        /// wtihout allowing it to look up the name of the method to identify the block.  This should only be used in
        /// conjunction with the _E function.
        /// This method is only present when the DEBUG contiditional is specified
        /// </summary>
        /// <param name="theMessage">The message to write</param>
        internal void InternalX(MessageMetadata mmd, string theMessage) {
            // Have removed the stack based method - while this method is less efficient it does work cross threads
            // previously the lookup was done on enter and the data pushed into a stack.  This did not work for multiple
            // threads, each of which can be in a different method.
            string timeString = null;
            if (sets.AddTimingsToEnterExit) {
                DateTime dt = DateTime.Now;
                timeString = ">TRCTMR<|END|" + dt.Day + "|" + dt.Month + "|" + dt.Year + "|" + dt.Hour + "|" + dt.Minute + "|" + dt.Second + "|" + dt.Millisecond + "|";
            }

            mmd.CommandType = TraceCommandTypes.TraceMessageOut;
            mmd.Body = "< " + mmd.MethodName + " (" + theMessage + ")";
            mmd.FurtherDetails = timeString;

            ActiveRouteMessage(mmd);
            InternalTimeCheckpoint(mmd, Constants.AUTOTIMER_PREFIX + mmd.MethodName, Constants.INTERNAL_CLASS_REPLACE, false);
        }

        /// <summary>
        /// InternalTime checkpoint method handles timer start and stop requests
        /// </summary>
        /// <param name="timerTitle">The instance timer to be used</param>
        /// <param name="timerSinkCategory">A generic category to total time for</param>
        /// <param name="timerStart">Is this a start or stop timer</param>
        internal void InternalTimeCheckpoint(MessageMetadata mmd, string timerTitle, string timerSinkCategory, bool timerStart) {
            // while this likes a duplicate it allows for other functions within Tex to call time skink

            DateTime timeCheck = DateTime.Now;

            if (timerSinkCategory == null) { timerSinkCategory = string.Empty; }

            string timerDataString = TraceMessageFormat.AssembleTimerContentString(timerTitle, timerSinkCategory, timeCheck);
            string timerEnterExit; //= timerStart ?  :
            if (timerStart) {
                mmd.CommandType = TraceCommandTypes.SectionStart;
                timerEnterExit = " Passed ";
            } else {
                mmd.CommandType = TraceCommandTypes.SectionEnd;
                timerEnterExit = " Exited ";
            }
            mmd.Body = String.Format(Constants.TIMER_SECTIONIDENTIFIER + "Time Checkpoint " + timerTitle + timerEnterExit + ". Time Entered {0,2}", timeCheck.ToString());
            mmd.FurtherDetails = timerDataString;
            ActiveRouteMessage(mmd);
        }


        // TODO: SOURCELEVELS
#if false

        internal bool IncludeThisMethodInOutput(Switch callSwitch, TraceLevel methodsLevel) {
            if (callSwitch == null) { return false; }

            BooleanSwitch bs = callSwitch as BooleanSwitch;
            if (bs != null) {
                return (bs.Enabled && IncludeThisMethodInOutput(methodsLevel));
            }

            return IncludeThisMethodInOutputFromSwitch(methodsLevel, ((TraceSwitch)callSwitch).Level);
        }

        internal bool IncludeThisMethodInOutputFromSwitch(TraceLevel methodLevel, TraceLevel switchLevel) {
            TraceLevel targetLevel = activeTraceLevel;
            if (switchLevel < activeTraceLevel) {
                targetLevel = switchLevel;
            }

            return (int)methodLevel <= (int)targetLevel;
        }
#endif

        /// <summary><para>
        /// Called within each trace method to determine whether or not the call should be included within the trace stream.
        /// </para></summary>>
        internal bool IncludeThisMethodInOutput(TraceLevel methodsLevel) {
#if DEBUG
            int ctl = (int)activeTraceLevel;
            if ((ctl < 0) || (ctl > 4)) { throw new InvalidOperationException("The code was written using the definition of the Trace enum which has ranges from 0..4"); }
#endif

            return (int)methodsLevel <= (int)activeTraceLevel;
        }

    }
}
