﻿using System.Threading.Tasks;

namespace FoxTunes
{
    public class BuildLibraryHierarchiesTask : LibraryTaskBase
    {
        public BuildLibraryHierarchiesTask(LibraryItemStatus? status)
            : base()
        {
            this.Status = status;
        }

        public LibraryItemStatus? Status { get; private set; }

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

        protected override async Task OnStarted()
        {
            await this.SetName("Building hierarchies").ConfigureAwait(false);
            await this.SetDescription("Preparing").ConfigureAwait(false);
            await this.SetIsIndeterminate(true).ConfigureAwait(false);
            await base.OnStarted().ConfigureAwait(false);
        }

        protected override async Task OnRun()
        {
            await this.BuildHierarchies(this.Status).ConfigureAwait(false);
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated)).ConfigureAwait(false);
        }
    }
}
