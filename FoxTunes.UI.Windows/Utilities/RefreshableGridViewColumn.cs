﻿using System.ComponentModel;
using System.Windows.Controls;

namespace FoxTunes.Utilities
{
    public class RefreshableGridViewColumn : GridViewColumn
    {
        public void Refresh()
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs("DisplayMemberBinding"));
        }
    }
}
