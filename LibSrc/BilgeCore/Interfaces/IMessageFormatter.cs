namespace Plisky.Diagnostics.Listeners {

    public interface IMessageFormatter {

        string ConvertToString(MessageMetadata msg);
    }
}