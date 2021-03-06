﻿using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FoxTunes.ViewModel
{
    public class MetaDataEntry : ViewModelBase
    {
        public static readonly string MyPictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        public static readonly string NoValue = "<No Value>";

        public static readonly string MultipleValues = "<Multiple Values>";

        public static readonly ThemeLoader ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();

        public static readonly ImageLoader ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();

        public static readonly IFileSystemBrowser FileSystemBrowser = ComponentRegistry.Instance.GetComponent<IFileSystemBrowser>();

        private MetaDataEntry()
        {
            this.Sources = new List<IFileData>();
            this.MetaDataItems = new Dictionary<IFileData, MetaDataItem>();
        }

        public MetaDataEntry(string name, MetaDataItemType type) : this()
        {
            this.Name = name;
            this.Type = type;
        }

        private IList<IFileData> Sources { get; set; }

        private IDictionary<IFileData, MetaDataItem> MetaDataItems { get; set; }

        private string _Name { get; set; }

        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
                this.OnNameChanged();
            }
        }

        protected virtual void OnNameChanged()
        {
            if (this.NameChanged != null)
            {
                this.NameChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Name");
        }

        public event EventHandler NameChanged;

        private MetaDataItemType _Type { get; set; }

        public MetaDataItemType Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this._Type = value;
                this.OnTypeChanged();
            }
        }

        protected virtual void OnTypeChanged()
        {
            if (this.TypeChanged != null)
            {
                this.TypeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Type");
        }

        public event EventHandler TypeChanged;

        public string OriginalValue { get; protected set; }

        private string _Value { get; set; }

        public string Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
                this.OnValueChanged();
            }
        }

        protected virtual void OnValueChanged()
        {
            if (string.Equals(this.Value, NoValue, StringComparison.OrdinalIgnoreCase))
            {
                this.HasValue = false;
            }
            else
            {
                this.HasValue = true;
            }
            if (string.Equals(this.Value, MultipleValues, StringComparison.OrdinalIgnoreCase))
            {
                this.HasMultipleValues = true;
            }
            else
            {
                this.HasMultipleValues = false;
            }
            this.OnHasChangesChanged();
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Value");
        }

        public event EventHandler ValueChanged;

        public void SetValue(string value)
        {
            this.Value = value;
            switch (this.Type)
            {
                case MetaDataItemType.Image:
                    this.RefreshImageSource();
                    break;
            }
        }

        public bool HasChanges
        {
            get
            {
                return !string.Equals(this.Value, this.OriginalValue);
            }
        }

        protected virtual void OnHasChangesChanged()
        {
            if (this.HasChangesChanged != null)
            {
                this.HasChangesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasChanges");
        }

        public event EventHandler HasChangesChanged;

        private ImageSource _ImageSource { get; set; }

        public ImageSource ImageSource
        {
            get
            {
                return this._ImageSource;
            }
            set
            {
                this._ImageSource = value;
                this.OnImageSourceChanged();
            }
        }

        protected virtual void OnImageSourceChanged()
        {
            if (this.ImageSourceChanged != null)
            {
                this.ImageSourceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ImageSource");
        }

        public event EventHandler ImageSourceChanged;

        private bool _HasValue { get; set; }

        public bool HasValue
        {
            get
            {
                return this._HasValue;
            }
            set
            {
                this._HasValue = value;
                this.OnHasValueChanged();
            }
        }

        protected virtual void OnHasValueChanged()
        {
            if (this.HasValueChanged != null)
            {
                this.HasValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasValue");
        }

        public event EventHandler HasValueChanged;

        private bool _HasMultipleValues { get; set; }

        public bool HasMultipleValues
        {
            get
            {
                return this._HasMultipleValues;
            }
            set
            {
                this._HasMultipleValues = value;
                this.OnHasMultipleValuesChanged();
            }
        }

        protected virtual void OnHasMultipleValuesChanged()
        {
            if (this.HasMultipleValuesChanged != null)
            {
                this.HasMultipleValuesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasMultipleValues");
        }

        public event EventHandler HasMultipleValuesChanged;

        public override void InitializeComponent(ICore core)
        {
            foreach (var key in this.MetaDataItems.Keys)
            {
                var metaDataItem = default(MetaDataItem);
                if (!this.MetaDataItems.TryGetValue(key, out metaDataItem) || metaDataItem == null)
                {
                    if (!this.HasValue)
                    {
                        this.Value = NoValue;
                    }
                    else
                    {
                        this.Value = MultipleValues;
                        break;
                    }
                }
                else if (!this.HasValue)
                {
                    this.Value = metaDataItem.Value;
                }
                else if (!string.Equals(this.Value, metaDataItem.Value))
                {
                    this.Value = MultipleValues;
                    break;
                }
            }
            this.OriginalValue = this.Value;
            switch (this.Type)
            {
                case MetaDataItemType.Image:
                    this.RefreshImageSource();
                    break;
            }
            base.InitializeComponent(core);
        }

        protected virtual void RefreshImageSource()
        {
            if (!this.HasValue || this.HasMultipleValues)
            {
                //TODO: The <No Value>/<Multiple Values> text is hard to read when we use the placeholder image.
                //this.ImageSource = ImageLoader.Load(
                //    ThemeLoader.Theme.Id,
                //    () => ThemeLoader.Theme.ArtworkPlaceholder,
                //    true
                //);
                this.ImageSource = null;
            }
            else
            {
                if (File.Exists(this.Value))
                {
                    this.ImageSource = ImageLoader.Load(
                        this.Value,
                        this.Value,
                        0,
                        0,
                        true
                    );
                }
            }
        }

        public IEnumerable<IFileData> GetSources()
        {
            return this.Sources;
        }

        public void SetSources(IEnumerable<IFileData> sources)
        {
            foreach (var source in sources)
            {
                this.AddSource(source);
            }
        }

        protected virtual void AddSource(IFileData source)
        {
            this.AddSource(
                source,
                source.MetaDatas.FirstOrDefault(
                    metaDataItem => string.Equals(metaDataItem.Name, this.Name, StringComparison.OrdinalIgnoreCase)
                )
            );
        }

        protected virtual void AddSource(IFileData source, MetaDataItem metaDataItem)
        {
            this.Sources.Add(source);
            if (metaDataItem != null)
            {
                this.MetaDataItems.Add(source, metaDataItem);
            }
        }

        public void Save()
        {
            if (!this.HasChanges || this.HasMultipleValues)
            {
                return;
            }
            foreach (var source in this.Sources)
            {
                var metaDataItem = default(MetaDataItem);
                if (!this.MetaDataItems.TryGetValue(source, out metaDataItem))
                {
                    metaDataItem = new MetaDataItem(this.Name, this.Type);
                    source.MetaDatas.Add(metaDataItem);
                    this.MetaDataItems.Add(source, metaDataItem);
                }
                if (this.HasValue)
                {
                    metaDataItem.Value = this.Value;
                }
                else
                {
                    metaDataItem.Value = null;
                }
            }
        }

        public ICommand BrowseCommand
        {
            get
            {
                return new Command(this.Browse, () => this.CanBrowse);
            }
        }

        public bool CanBrowse
        {
            get
            {
                return this.Sources.Count > 0;
            }
        }

        public void Browse()
        {
            var directoryName = this.Sources.Select(
                playlistItem => playlistItem.DirectoryName
            ).FirstOrDefault();
            if (string.IsNullOrEmpty(directoryName))
            {
                directoryName = MyPictures;
            }
            var options = new BrowseOptions(
                "Select Artwork",
                directoryName,
                new[]
                {
                    new BrowseFilter("Images", ArtworkProvider.EXTENSIONS)
                },
                BrowseFlags.File
            );
            var result = FileSystemBrowser.Browse(options);
            if (!result.Success)
            {
                return;
            }
            this.SetValue(result.Paths.FirstOrDefault());
        }

        public ICommand DragEnterCommand
        {
            get
            {
                return new Command<DragEventArgs>(this.OnDragEnter);
            }
        }

        protected virtual void OnDragEnter(DragEventArgs e)
        {
            this.UpdateDragDropEffects(e);
        }

        public ICommand DragOverCommand
        {
            get
            {
                return new Command<DragEventArgs>(this.OnDragOver);
            }
        }

        protected virtual void OnDragOver(DragEventArgs e)
        {
            this.UpdateDragDropEffects(e);
        }

        protected virtual void UpdateDragDropEffects(DragEventArgs e)
        {
            var effects = DragDropEffects.None;
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    effects = DragDropEffects.Copy;
                }
#if VISTA
                if (ShellIDListHelper.GetDataPresent(e.Data))
                {
                    effects = DragDropEffects.Copy;
                }
#endif
            }
            catch (Exception exception)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to query clipboard contents: {0}", exception.Message);
            }
            e.Effects = effects;
        }

        public ICommand DropCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand<DragEventArgs>(
                    new Func<DragEventArgs, Task>(this.OnDrop)
                );
            }
        }

        protected virtual Task OnDrop(DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var paths = e.Data.GetData(DataFormats.FileDrop) as IEnumerable<string>;
                    this.SetValue(paths.FirstOrDefault());
                }
#if VISTA
                if (ShellIDListHelper.GetDataPresent(e.Data))
                {
                    var paths = ShellIDListHelper.GetData(e.Data);
                    this.SetValue(paths.FirstOrDefault());
                }
#endif
            }
            catch (Exception exception)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to process clipboard contents: {0}", exception.Message);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public ICommand ClearCommand
        {
            get
            {
                return new Command(this.Clear, () => this.CanClear);
            }
        }

        public bool CanClear
        {
            get
            {
                return this.Sources.Count > 0 && this.HasValue;
            }
        }

        public void Clear()
        {
            this.Value = NoValue;
            switch (this.Type)
            {
                case MetaDataItemType.Image:
                    this.RefreshImageSource();
                    break;
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MetaDataEntry();
        }
    }
}
