﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IOutput : IStandardComponent
    {
        string Name { get; }

        string Description { get; }

        bool IsStarted { get; }

        bool ShowBuffering { get; }

        event AsyncEventHandler IsStartedChanged;

        Task Start();

        IEnumerable<string> SupportedExtensions { get; }

        bool IsSupported(string fileName);

        Task<IOutputStream> Load(PlaylistItem playlistItem, bool immidiate);

        Task<bool> Preempt(IOutputStream stream);

        Task Unload(IOutputStream stream);

        Task Shutdown();

        int GetData(float[] buffer);
    }
}
