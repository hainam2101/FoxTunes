﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPathsToPlaylistTask : PlaylistTaskBase
    {
        public AddPathsToPlaylistTask(int sequence, IEnumerable<string> paths, bool clear)
            : base(sequence)
        {
            this.Paths = paths;
            this.Clear = clear;
        }

        public override bool Visible
        {
            get
            {
                return true;
            }
        }

        public override bool Cancellable
        {
            get
            {
                return true;
            }
        }

        public IEnumerable<string> Paths { get; private set; }

        public bool Clear { get; private set; }

        protected override async Task OnStarted()
        {
            await this.SetName("Getting file list").ConfigureAwait(false);
            await this.SetIsIndeterminate(true).ConfigureAwait(false);
            await base.OnStarted().ConfigureAwait(false);
        }

        protected override async Task OnRun()
        {
            if (this.Clear)
            {
                await this.RemoveItems(PlaylistItemStatus.None).ConfigureAwait(false);
            }
            await this.AddPaths(this.Paths).ConfigureAwait(false);

        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated)).ConfigureAwait(false);
        }
    }
}
