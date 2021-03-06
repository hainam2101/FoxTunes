﻿using System.Collections.ObjectModel;

namespace FoxTunes.Interfaces
{
    public interface IConfiguration : IStandardComponent
    {
        ReleaseType ReleaseType { get; }

        ObservableCollection<ConfigurationSection> Sections { get; }

        void RegisterSection(ConfigurationSection section);

        void Load();

        void Save();

        void Reset();

        ConfigurationSection GetSection(string sectionId);

        ConfigurationElement GetElement(string sectionId, string elementId);

        T GetElement<T>(string sectionId, string elementId) where T : ConfigurationElement;
    }

    public enum ReleaseType : byte
    {
        Default = 0,
        Minimal = 1
    }
}
