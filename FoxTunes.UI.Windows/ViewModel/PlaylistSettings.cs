﻿using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class PlaylistSettings : ViewModelBase
    {
        public PlaylistColumnManager PlaylistColumnManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private CollectionManager<PlaylistColumn> _PlaylistColumns { get; set; }

        public CollectionManager<PlaylistColumn> PlaylistColumns
        {
            get
            {
                return this._PlaylistColumns;
            }
            set
            {
                this._PlaylistColumns = value;
                this.OnPlaylistColumnsChanged();
            }
        }

        protected virtual void OnPlaylistColumnsChanged()
        {
            this.OnPropertyChanged("PlaylistColumns");
        }

        private ObservableCollection<UIPlaylistColumnProvider> _PlaylistColumnProviders { get; set; }

        public ObservableCollection<UIPlaylistColumnProvider> PlaylistColumnProviders
        {
            get
            {
                return this._PlaylistColumnProviders;
            }
            set
            {
                this._PlaylistColumnProviders = value;
                this.OnPlaylistColumnProvidersChanged();
            }
        }

        protected virtual void OnPlaylistColumnProvidersChanged()
        {
            this.OnPropertyChanged("PlaylistColumnProviders");
        }

        public bool IsSaving
        {
            get
            {
                return global::FoxTunes.BackgroundTask.Active
                    .OfType<PlaylistTaskBase>()
                    .Any();
            }
        }

        protected virtual void OnIsSavingChanged()
        {
            if (this.IsSavingChanged != null)
            {
                this.IsSavingChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsSaving");
        }

        public event EventHandler IsSavingChanged;

        public ICommand SaveCommand
        {
            get
            {
                var command = CommandFactory.Instance.CreateCommand(
                    new Func<Task>(this.Save)
                );
                command.Tag = CommandHints.DISMISS;
                return command;
            }
        }

        public async Task Save()
        {
            var exception = default(Exception);
            try
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var task = new SingletonReentrantTask(CancellationToken.None, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, cancellationToken =>
                    {
                        using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                        {
                            var playlistColumns = database.Set<PlaylistColumn>(transaction);
                            playlistColumns.Remove(playlistColumns.Except(this.PlaylistColumns.ItemsSource));
                            playlistColumns.AddOrUpdate(this.PlaylistColumns.ItemsSource);
                            transaction.Commit();
                        }
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }))
                    {
                        await task.Run().ConfigureAwait(false);
                    }
                }
                await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistColumnsUpdated)).ConfigureAwait(false);
                return;
            }
            catch (Exception e)
            {
                exception = e;
            }
            await this.OnError("Save", exception).ConfigureAwait(false);
            throw exception;
        }

        public ICommand CancelCommand
        {
            get
            {
                return new Command(this.Cancel)
                {
                    Tag = CommandHints.DISMISS
                };
            }
        }

        public void Cancel()
        {
            var task = this.Refresh();
        }

        public ICommand ResetCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.Reset);
            }
        }

        public async Task Reset()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var task = new SingletonReentrantTask(CancellationToken.None, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, cancellationToken =>
                {
                    this.PlaylistManager.InitializeDatabase(database);
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }))
                {
                    await task.Run().ConfigureAwait(false);
                }
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistColumnsUpdated)).ConfigureAwait(false);
            await this.Refresh().ConfigureAwait(false);
        }

        public override void InitializeComponent(ICore core)
        {
            global::FoxTunes.BackgroundTask.ActiveChanged += this.OnActiveChanged;
            this.PlaylistColumnManager = ComponentRegistry.Instance.GetComponent<PlaylistColumnManager>();
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.DatabaseFactory = this.Core.Factories.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.PlaylistColumns = new CollectionManager<PlaylistColumn>()
            {
                ItemFactory = () =>
                {
                    using (var database = this.DatabaseFactory.Create())
                    {
                        return database.Set<PlaylistColumn>().Create().With(playlistColumn =>
                        {
                            playlistColumn.Name = "New";
                            playlistColumn.Script = "'New'";
                            playlistColumn.Sequence = this.PlaylistColumns.ItemsSource.Count();
                        });
                    }
                },
                ExchangeHandler = (item1, item2) =>
                {
                    var temp = item1.Sequence;
                    item1.Sequence = item2.Sequence;
                    item2.Sequence = temp;
                }
            };
            this.PlaylistColumnProviders = new ObservableCollection<UIPlaylistColumnProvider>(
                this.PlaylistColumnManager.GetProviders()
            );
            var task = this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual async void OnActiveChanged(object sender, EventArgs e)
        {
            await Windows.Invoke(() => this.OnIsSavingChanged()).ConfigureAwait(false);
        }

        private Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.SettingsUpdated:
                case CommonSignals.PlaylistColumnsUpdated:
                    return this.Refresh();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual Task Refresh()
        {
            return Windows.Invoke(() =>
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        this.PlaylistColumns.ItemsSource = new ObservableCollection<PlaylistColumn>(
                            database.Set<PlaylistColumn>(transaction)
                        );
                    }
                }
            });
        }

        protected override void OnDisposing()
        {
            global::FoxTunes.BackgroundTask.ActiveChanged -= this.OnActiveChanged;
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PlaylistSettings();
        }
    }
}
