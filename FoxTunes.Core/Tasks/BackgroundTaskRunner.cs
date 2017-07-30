﻿using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes.Tasks
{
    public class BackgroundTaskRunner : StandardComponent, IBackgroundTaskRunner
    {
        public Task Run(Action action)
        {
            return Task.Factory.StartNew(action);
        }
    }
}