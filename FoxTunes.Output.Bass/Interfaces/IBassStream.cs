﻿using ManagedBass;

namespace FoxTunes.Interfaces
{
    public interface IBassStream
    {
        IBassStreamProvider Provider { get; }

        int ChannelHandle { get; }

        bool IsEmpty { get; }

        Errors Errors { get; }
    }
}
