﻿using System.Threading.Tasks;

namespace Plisky.Diagnostics {

    public interface IBilgeMessageHandler {
        int Priority { get; set; }
        string Name { get; }

        string GetStatus();

        void SetFormatter(IMessageFormatter fmt);

        Task HandleMessageAsync(MessageMetadata[] msg);

        void HandleMessage40(MessageMetadata[] msg);

        void Flush();

        /// <summary>
        /// This is a custom dispose implementation because the internals will call dispose but
        /// that causes FxCop style rules therefore this method is used to clear resources instead.
        /// </summary>
        void CleanUpResources();
    }
}