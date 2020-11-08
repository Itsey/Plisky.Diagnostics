namespace Plisky.Diagnostics {


    /// <summary>
    /// IMessageFormatter is an interface used by the listeners to convert a structured MesageMetaData into a string representation for transport or display,
    /// therea are several built in message formatters but others can be used too.  The most common ones are for console, readable text file and import into
    /// FlimFlam either using the legacy formatter or the V2 Formatter.
    /// </summary>
    public interface IMessageFormatter {
        
        /// <summary>
        /// Converts the Message structure to a string using the formatter information
        /// </summary>
        /// <param name="msg">The message structure to convert</param>
        /// <returns>String representation of the message structure</returns>
        string Convert(MessageMetadata msg);

        string ConvertWithReference(MessageMetadata msg, string uniquenessReference);

    }
}