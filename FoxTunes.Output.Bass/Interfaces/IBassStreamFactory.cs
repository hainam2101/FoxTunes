﻿using ManagedBass;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamFactory : IBaseComponent
    {
        IEnumerable<IBassStreamProvider> Providers { get; }

        void Register(IBassStreamProvider provider);

        IEnumerable<IBassStreamProvider> GetProviders(PlaylistItem playlistItem);

        Task<IBassStream> CreateStream(PlaylistItem playlistItem, bool immidiate);

        Task<IBassStream> CreateStream(PlaylistItem playlistItem, bool immidiate, BassFlags flags);
    }
}
