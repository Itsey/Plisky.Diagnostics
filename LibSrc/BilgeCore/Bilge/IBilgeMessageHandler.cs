using System.Threading.Tasks;

namespace Plisky.Diagnostics {

    public interface IBilgeMessageHandler {
        int Priority { get; }
        string Name { get;  }
        //void HandleMessage(MessageMetadata msg);

#if NET452 || NETSTANDARD2_0
        Task HandleMessageAsync(MessageMetadata[] msg);
#else
        void HandleMessage40(MessageMetadata[] msg);
#endif
        void Flush();
    }
}