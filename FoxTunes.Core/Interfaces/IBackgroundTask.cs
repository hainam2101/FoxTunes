﻿using System;

namespace FoxTunes.Interfaces
{
    public interface IBackgroundTask
    {
        string Id { get; }

        string Name { get; }

        event EventHandler NameChanged;

        string Description { get; }

        event EventHandler DescriptionChanged;

        int Position { get; }

        event EventHandler PositionChanged;

        int Count { get; }

        event EventHandler CountChanged;

        event EventHandler Started;

        event EventHandler Completed;
    }
}