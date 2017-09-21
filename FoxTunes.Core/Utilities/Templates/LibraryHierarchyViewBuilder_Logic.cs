﻿using System.Collections.Generic;
using System.Linq;

namespace FoxTunes.Utilities.Templates
{
    public partial class LibraryHierarchyViewBuilder
    {
        public LibraryHierarchyViewBuilder(IEnumerable<string> metaDataNames)
        {
            this.MetaDataNames = metaDataNames.ToArray();
        }

        public string[] MetaDataNames { get; private set; }
    }
}
