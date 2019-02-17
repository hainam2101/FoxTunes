﻿using FoxDb.Interfaces;
using System;

namespace FoxTunes.Interfaces
{
    public interface ICore : IDisposable
    {
        IStandardComponents Components { get; }

        IStandardManagers Managers { get; }

        IStandardFactories Factories { get; }

        void Load();

        void CreateDefaultData(IDatabase database);
    }
}
