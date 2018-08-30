using System.Threading.Tasks;

namespace Plisky.Diagnostics {

    public interface IBilgeMessageHandler {
        int Priority { get; }
        string Name { get; }

        string GetStatus();

#if NET452 || NETSTANDARD2_0
        Task HandleMessageAsync(MessageMetadata[] msg);
#else
        void HandleMessage40(MessageMetadata[] msg);
#endif
        void Flush();

        /// <summary>
        /// This is a custom dispose implementation because the internals will call dispose but
        /// that causes FxCop style rules therefore this method is used to clear resources instead.
        /// </summary>
        void CleanUpResources();
    }
}