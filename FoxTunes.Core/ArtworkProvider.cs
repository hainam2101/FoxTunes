﻿using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class ArtworkProvider : StandardComponent, IArtworkProvider
    {
        public static IDictionary<ArtworkType, string[]> Names = GetNames();

        private static IDictionary<ArtworkType, string[]> GetNames()
        {
            return new Dictionary<ArtworkType, string[]>()
            {
                { ArtworkType.FrontCover, new [] { "front", "cover", "folder" } },
                { ArtworkType.BackCover, new [] { "back" } }
            };
        }

        public MetaDataItem Find(string path, ArtworkType type)
        {
            var names = default(string[]);
            if (!Names.TryGetValue(type, out names))
            {
                throw new NotImplementedException();
            }
            var directoryName = Path.GetDirectoryName(path);
            foreach (var name in names)
            {
                foreach (var fileName in Directory.EnumerateFileSystemEntries(directoryName, string.Format("{0}.*", name)))
                {
                    return new MetaDataItem(Enum.GetName(typeof(ArtworkType), type), MetaDataItemType.Image)
                    {
                        FileValue = fileName
                    };
                }
            }
            return default(MetaDataItem);
        }

        public MetaDataItem Find(PlaylistItem playlistItem, ArtworkType type)
        {
            return playlistItem.MetaDatas.FirstOrDefault(
                 metaDataItem => 
                     metaDataItem.Type == MetaDataItemType.Image && 
                     string.Equals(metaDataItem.Name, Enum.GetName(typeof(ArtworkType), type), StringComparison.OrdinalIgnoreCase) && 
                     File.Exists(metaDataItem.FileValue)
             );
        }
    }
}