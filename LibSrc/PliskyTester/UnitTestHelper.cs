
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Plisky.Helpers {

    /// <summary>
    /// Class designed to support the adoption of unit tests and provide helper functions for typical common elements.
    /// </summary>
    public sealed class UnitTestHelper {
        private Random rand;

        /// <summary>
        /// Provides access to a Random class stored within the unit test helper.  No benefit to using it over a normal one
        /// just saves having to create them or store them.
        /// </summary>
        public Random RandomStore {
            get {
                if (rand == null) {
                    rand = new Random();
                }

                return rand;
            }
        }

        /// <summary>
        /// Generic string used in unit testing, when any old token string will do.
        /// </summary>
        public const string GenericString1 = "arflebarflegloop";

        /// <summary>
        /// Second Generic string used in unit testing, when any old token string will do.
        /// </summary>
        public const string GenericString2 = "BilgeAndFlimflam";

        /// <summary>
        /// Third Generic string used in unit testing, when any old token string will do.
        /// </summary>
        public const string GenericString3 = "spontralification of the spire";

        /// <summary>
        /// Creates a new instance of the UnitTestHelper class
        /// </summary>
        public UnitTestHelper() {
            CaseSensitiveMatches = false;
        }

        ~UnitTestHelper() {
            ClearUpTestFiles();
        }

        /// <summary>
        /// Gets or Sets a value which determines whether the string comparisons performed within the helper are case insensitive or not.
        /// </summary>
        /// <remarks>Defaults to false, making all matches insensitive</remarks>
        public bool CaseSensitiveMatches { get; set; }

        private List<string> m_storedFilenames = new List<string>();

        /// <summary>
        /// Returns a TemporaryFilename which can be used for storing data during unit tests.  This filename is stored within
        /// the UnitTestHelper class such that it can be cleaned up with a call to ClearUpTestFiles.
        /// </summary>
        /// <returns></returns>
        public string NewTemporaryFileName(bool deleteOnCreate = false) {
            string result = Path.GetTempFileName();
            m_storedFilenames.Add(result);
            if (deleteOnCreate) {
                File.Delete(result);
            }
            return result;
        }

        internal void RegisterTemporaryFilename(string tmp) {
            m_storedFilenames.Add(tmp);
        }

        /// <summary>
        /// Compares two objects to see if they are alike, very simple implementation for top levle only at the moment, should
        /// be expanded to use recursion to compare any objects
        /// </summary>
        /// <param name="target1">First object for comparison</param>
        /// <param name="target2">Second object for comparison</param>
        /// <returns>True if they are the same</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This confuses the interface considerably and its only a unit test helper therefore performance not important")]
        public bool ReflectionBasedCompare(object target1, object target2) {
            // Either they are both null or neither are
            if ((target1 == null) && (target2 == null)) { return true; }
            if ((target1 == null) && (target2 != null)) { return false; }
            if ((target2 == null) && (target1 != null)) { return false; }
            // Now they must be of the same type
            Type t = target1.GetType();
            if (t != target2.GetType()) { return false; }

            if (target1.GetType().IsArray) {
                return CompareArrays((Array)target1, (Array)target2);
            }

            if (target1.GetType().IsValueType) {
                return target1.Equals(target2);
            }

            if (target1.GetType() == typeof(string)) {
                return target1.Equals(target2);
            }

            foreach (PropertyInfo pi in t.GetProperties()) {
                if (pi.GetIndexParameters().Length == 0) {
                    // Non indexed properties

                    object v1 = pi.GetValue(target1, null);
                    object v2 = pi.GetValue(target2, null);
                    // If either one of them is null then they must both be null
                    if ((v1 == null) || (v2 == null)) {
                        if ((v1 == null) && (v2 != null)) { return false; }
                        if ((v2 == null) && (v1 != null)) { return false; }
                    } else {
                        if (ReflectionBasedCompare(v1, v2) != true) {
                            return false;
                        }
                    }
                } else {
                    // This is for indexed properties.
                    //Bilge.Log("Skipping indexed property");
                }
            }

            foreach (FieldInfo fi in t.GetFields()) {
                object v1 = fi.GetValue(target1);
                object v2 = fi.GetValue(target2);

                if ((v1 == null) || (v2 == null)) {
                    if ((v1 == null) && (v2 != null)) { return false; }
                    if ((v2 == null) && (v1 != null)) { return false; }
                } else {
                    if (v1.GetType().IsArray) {
                        if (!v2.GetType().IsArray) {
                            return false;
                        }

                        // This is not a very comprehensive test for equality, if they are arrays and they
                        // are the same length then we guess they are the same.
                    } else {
                        if (v1.Equals(v2) == false) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static bool CompareArrays(Array o1, Array o2) {
            return o1.Length == o2.Length;
        }

        /// <summary>
        /// Designed to use reflection to work out the type of a value passed to it then alter that value to something different, aimed at
        /// changing values for unit tests.  All that can be said is that the return value will not equal the entry value for supported simple types.
        /// </summary>
        /// <remarks>If the val object is of an unsupported type it is returned unchanged.</remarks>
        /// <param name="target">The value to change</param>
        /// <returns>A new value for the object val</returns>
        public object AlterValue(object target) {
            if (target == null) { return null; }

            Type t = target.GetType();
            if (t == typeof(long)) {
                return (long)target + 1;
            }
            if (t == typeof(uint)) {
                return (uint)target + 1;
            }
            if (t == typeof(double)) {
                return (double)target + 1;
            }
            if (t == typeof(int)) {
                return (int)target + 1;
            }

            if (t == typeof(bool)) {
                return (!(bool)target);  // Invert it for bools
            }
            if (t == typeof(string)) {
                string s = GenerateFriendlyString();
                int length = RandomStore.Next(5);
                if (s.Length < length) {
                    length = s.Length;
                }
                return (string)target + s.Substring(0, length);
            }
            return target;
        }

        private int m_limitStringsTo = 2000;

        /// <summary>
        /// The maximum length which random strings are returned for any method which is called without
        /// specifying the maximum
        /// </summary>
        public int LimitStringsTo {
            get { return m_limitStringsTo; }
            set { m_limitStringsTo = value; }
        }

        /// <summary>
        /// Returns a generated filename safe string between the lengths of 3 and 30 characters.
        /// </summary>
        /// <returns>A random, filename friendly string between 3 and 30 characters long</returns>
        public string GenerateFriendlyString() {
            return GenerateSpecificRandomString(3, 30, false, true);
        }

        /// <summary>
        /// Generates a random string between the specified minimum and maximum lengths,
        /// </summary>
        /// <param name="minLength">The minimum length of the returned string, can be 0 or greater</param>
        /// <param name="maxLength">The maximum length of the returned string must be >= minLength</param>
        /// <param name="makeFileNameSafe">If true only filename safe characters are returned</param>
        /// <returns>A randomly generated string between minLength and maxLength characters in length.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if min length is less than zero or greater than max length</exception>
        public string GenerateString(int minLength, int maxLength, bool makeFileNameSafe) {
            if (makeFileNameSafe) {
                return GenerateSpecificRandomString(minLength, maxLength, false, true);
            } else {
                return GenerateSpecificRandomString(minLength, maxLength, false, true);
            }
        }

        public string GenerateString(int minLength, int maxLength, bool makeFileNameSafe, bool useNumbers) {
            return GenerateSpecificRandomString(minLength, maxLength, makeFileNameSafe, useNumbers);
        }

        private string GenerateSpecificRandomString(int minLength, int maxLength, bool allowPunctuation, bool allowNumbers) {

            #region entry code

            if (minLength < 0) {
                throw new ArgumentOutOfRangeException("minLength", "minLength must be 0 or greater");
            }
            if (minLength > maxLength) {
                throw new ArgumentOutOfRangeException("minLength", "minLength must be less than maxLength");
            }
            if (maxLength == 0) {
                return string.Empty;
            }

            #endregion

            string sampleRange = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            if (allowPunctuation) {
                sampleRange += ",.<>;':@#/~?+_-=!\"£$%^&*()`¬|\\";
            }
            if (allowNumbers) {
                sampleRange += "1234567890";
            }

            char[] possibleCharacters = sampleRange.ToCharArray();

            int characters = rand.Next(minLength, maxLength);

            StringBuilder result = new StringBuilder(characters);   // I seriously doubt this is faster.

            for (; characters > 0; characters--) {
                result.Append(possibleCharacters[rand.Next(0, possibleCharacters.Length)]);
            }

            return result.ToString();
        }

        private string GenerateRandomString(int minLength, int maxLength) {
            int characters = RandomStore.Next(minLength, maxLength);

            StringBuilder result = new StringBuilder(characters);   // I seriously doubt this is faster.

            for (; characters > 0; characters--) {
                result.Append((char)RandomStore.Next(15, 125));
            }

            return result.ToString();
        }

        /// <summary>
        /// Generates a random string with which testing can be performed.  This string will contain a variety
        /// of characters within the ASCII caharacter range 15-125 ish.
        /// </summary>
        /// <returns>A random length generated string</returns>
        public string GenerateString() {
            return GenerateRandomString(1, m_limitStringsTo);
        }

        /// <summary>
        /// Generates a random string with which testing can be performed.  This string will contain a variety
        /// of characters within the ASCII caharacter range 15-125 ish.
        /// </summary>
        /// <param name="minimumLength"> The minimum length of this string</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the minimum Length specified is less than 0 or > LimitStringsTo</exception>
        /// <returns>A random length generated string</returns>
        public string GenerateString(int minimumLength) {
            if (minimumLength < 0) {
                throw new ArgumentOutOfRangeException("minimumLength", "The minimum length for a generated string can not be less than zero");
            }
            if (minimumLength > m_limitStringsTo) {
                throw new ArgumentOutOfRangeException("minimumLength", "The minimum length for a generated string exceeds the LimitStringsTo property length");
            }
            return GenerateRandomString(minimumLength, m_limitStringsTo);
        }

        /// <summary>
        /// Generates a random string with which testing can be performed.  This string will contain a variety
        /// of characters within the ASCII caharacter range 15-125 ish.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the minimum length or maximum length are specified as less than zero</exception>
        /// <param name="minimumLength">The minimum length of this string</param>
        /// <param name="maximumLength">The maximum length of this string</param>
        /// <returns>A random length generated string</returns>
        public string GenerateString(int minimumLength, int maximumLength) {
            if (minimumLength < 0) {
                throw new ArgumentOutOfRangeException("minimumLength", "The minimum length for a generated string can not be less than zero");
            }
            if (maximumLength < 0) {
                throw new ArgumentOutOfRangeException("minimumLength", "The maximum length for a generated string can not be less than zero");
            }
            if (maximumLength < minimumLength) {
                throw new ArgumentOutOfRangeException("maximumLength", "The maximum length must be greater than the minimum length");
            }

            return GenerateRandomString(minimumLength, maximumLength);
        }

        /// <summary>
        /// Attempts to delete all of the test files that are used.
        /// </summary>
        public void ClearUpTestFiles() {
            string nextFile = null;
            try {
                foreach (string s in m_storedFilenames) {
                    nextFile = s;
                    File.Delete(nextFile);
                }
            } catch (IOException) {
                //Bilge.Dump(iox, "Exception trying to clear up file:" + nextFile);
            }
        }

        /// <summary>
        /// Changes all of the values on a type a little bit.  This will incrememt numbers, alter strings and try and change all of the
        /// values on any object that is passed in.  This is purely designed for testing to change fields.
        /// </summary>
        /// <param name="target">The object to alter.</param>
        public void AlterAllValuesOnType(object target) {
            Type t = target.GetType();
            object val;
            object newOne;

            foreach (PropertyInfo pi in t.GetProperties()) {
                if (pi.CanWrite) {
                    val = pi.GetValue(target, null);
                    newOne = AlterValue(val);
                    pi.SetValue(target, newOne, null);
                }
            }
            foreach (FieldInfo fi in t.GetFields()) {
                val = fi.GetValue(target);
                newOne = AlterValue(val);
                fi.SetValue(target, newOne);
            }
        }

        /// <summary>
        /// Verifies that a string contains all of the partial string matches that are passed in the required elements parameter.
        /// </summary>
        /// <param name="dataToTest">The string to check for all words</param>
        /// <param name="requiredElements">The array of words</param>
        /// <remarks>This function is not case sensitive.</remarks>
        /// <returns>True if all of the partial strings are found, false otherwise</returns>
        public bool StringContainsAll(string dataToTest, params string[] requiredElements) {
            for (int i = 0; i < requiredElements.Length; i++) {
                if (!StringContains(dataToTest, requiredElements[i])) {

                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines whether a string loosely conforms to a set of ideas, the ideas are passed as strings to the
        /// method and these strings must be mentioned and must be mentioned in order so that the gist holds true.
        /// If any of the strings are not present or if they are present in the wrong order then this returns false.
        /// </summary>
        /// <remarks>This function is not case sensitive.</remarks>
        /// <param name="valueToCheck">The name of the string to parse</param>
        /// <param name="gistElements">Each of the gist elements to check for</param>
        /// <returns>True if each is found in order, false otherwise.</returns>
        public static bool StringContainsAllInOrder(string valueToCheck, params string[] gistElements) {
            int indexOfLast = 0;

            valueToCheck = valueToCheck.ToUpper();
            for (int i = 0; i < gistElements.Length; i++) {
                string s = gistElements[i].ToUpper();

                int index = valueToCheck.IndexOf(s);
                if (index < indexOfLast) {
                    // This is the case if the match is not met or if its met out of order.
                    return false;
                }
                indexOfLast = index;
            }
            return true;
        }

        /// <summary>
        /// Returns true if the specified filename is readonly.
        /// </summary>
        /// <param name="fileNameToCheck">The filename to check</param>
        /// <returns>True if the file is readonly.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the filename is empty or null</exception>
        public static bool IsReadOnly(string fileNameToCheck) {
            if ((fileNameToCheck == null) || (fileNameToCheck.Length == 0)) {
                throw new ArgumentException("The fileName must be valid.", "fileNameToCheck");
            }
            FileAttributes fa = File.GetAttributes(fileNameToCheck);
            return ((fa & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
        }

        private bool StringContains(string doesThis, string containThis) {
            if (this.CaseSensitiveMatches) {
                return doesThis.Contains(containThis);
            } else {
                return doesThis.ToLower().Contains(containThis.ToLower());
            }
        }

#if false

           /// Returns true or false.
        /// </summary>
        /// <returns>True or False</returns>
        public bool HeadsTails() {
            if (m_random.Next(1000) >= 500) {
                return true;
            } else {
                return false;
            }
        }
#endif

        /// <summary>
        /// Performs a match on matchData within searchData, checking each of the search strings to see if it contains an isntance
        /// of the match strings.  This method checks the matchData strings in the order specified in the array and will not return
        /// true if the matches occur out of order.
        /// </summary>
        /// <remarks>The match strings are matched only once, and all comparisons are case insensitive</remarks>
        /// <param name="searchData">The list of strings to check</param>
        /// <param name="matchData">The list of strings to find, in the specified order in the searchData strings</param>
        /// <returns>True if each of the matchData strings were found within the searchData strings in the correct order</returns>
        public bool SearchDataContainsMatchDataInOrder(string[] searchData, string[] matchData) {
            bool[] haveMatched = new bool[matchData.Length];
            bool[] haveConsumed = new bool[searchData.Length];

            int searchCount = 0;

            for (int matchCheckCount = 0; matchCheckCount < matchData.Length; matchCheckCount++) {
                if (haveMatched[matchCheckCount]) { continue; }

                for (; searchCount < searchData.Length; searchCount++) {
                    if (haveConsumed[searchCount]) { continue; }
                    if (StringContains(searchData[searchCount], matchData[matchCheckCount])) {
                        haveConsumed[searchCount] = true;
                        haveMatched[matchCheckCount] = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < haveMatched.Length; i++) {
                if (haveMatched[i] == false) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Performs a match on matchData within searchData, checking each of the search strings to see if it contains an isntance
        /// of the match strings.  This method checks the matchData strings and will return true if they are all found, no matter
        /// what the order of the matching is.
        /// </summary>
        /// <remarks>The match strings are matched only once, and all comparisons are based on the value of CaseSensiteMatches property </remarks>
        /// <param name="searchData">The list of strings to check</param>
        /// <param name="matchData">The list of strings to find</param>
        /// <returns>True if each of the matchData strings were found within the searchData strings in the correct order</returns>
        public bool SearchDataContainsMatchData(string[] searchData, string[] matchData) {
            bool[] haveMatched = new bool[matchData.Length];
            bool[] haveConsumed = new bool[searchData.Length];

            for (int matchCheckCount = 0; matchCheckCount < matchData.Length; matchCheckCount++) {
                if (haveMatched[matchCheckCount]) { continue; }

                for (int searchCount = 0; searchCount < searchData.Length; searchCount++) {
                    if (haveConsumed[searchCount]) { continue; }
                    if (StringContains(searchData[searchCount], matchData[matchCheckCount])) {
                        haveConsumed[searchCount] = true;
                        haveMatched[matchCheckCount] = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < haveMatched.Length; i++) {
                if (haveMatched[i] == false) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Reverses the case of a string passed, making all lower case letters upper case and all upper case letters lower case.
        /// </summary>
        /// <param name="data">The string to have its case reversed</param>
        /// <returns>The string with its case reversed.</returns>
        public static string ReverseCase(string data) {
            StringBuilder result = new StringBuilder(data.Length);

            foreach (char c in data) {
                result.Append(char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c));
            }

            return result.ToString();
        }

#if false
        static object Process(object obj) {
            if (obj == null)
                return null;
            Type type = obj.GetType();
            if (type.IsValueType || type == typeof(string)) {
                return obj;
            } else if (type.IsArray) {
                Type elementType = Type.GetType(
                     type.FullName.Replace("[]", string.Empty));
                var array = obj as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++) {
                    copied.SetValue(Process(array.GetValue(i)), i);
                }
                return Convert.ChangeType(copied, obj.GetType());
            } else if (type.IsClass) {
                object toret = Activator.CreateInstance(obj.GetType());
                FieldInfo[] fields = type.GetFields(BindingFlags.Public |
                            BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields) {
                    object fieldValue = field.GetValue(obj);
                    if (fieldValue == null)
                        continue;
                    field.SetValue(toret, Process(fieldValue));
                }
                return toret;
            } else
                throw new ArgumentException("Unknown type");
        }
#endif

        private object CloneObjectImplementation(object source) {
            if (source == null) { return null; }

            Type srcType = source.GetType();
            BindingFlags flgs = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            object result;

            if (srcType == typeof(string)) {
                // String must be checked for ahead of class even though its less likely.
                return string.Copy(source as string);
            }

            if (srcType.IsArray) {
                //Again must check before clas.
                Type arrayelementType = Type.GetTypeArray((object[])source)[0];
                Array sauce = (Array)source;
                Array newArray = Array.CreateInstance(arrayelementType, sauce.Length);
                for (int idx = 0; idx < newArray.Length; idx++) {
                    newArray.SetValue(CloneObjectImplementation(sauce.GetValue(idx)), idx);
                }
                return (object)newArray;  //Convert.ChangeType(newArray,srcType);
            }

            if (srcType.IsClass) {
                result = Activator.CreateInstance(srcType);
                foreach (var f in srcType.GetFields(flgs)) {
                    object actualField = f.GetValue(source);
                    if (actualField == null) {
                        f.SetValue(result, null);
                    } else {
                        f.SetValue(result, CloneObjectImplementation(actualField));
                    }
                }
                return result;
            }

            if (srcType.IsValueType) {
                return source;
            }

            throw new NotImplementedException();
        }

        public T CloneObject<T>(T source) {
            if (source == null) { throw new ArgumentNullException("source", "The source parameter can not be null when cloning."); }
            return (T)CloneObjectImplementation(source);
        }
    }
}