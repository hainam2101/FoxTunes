﻿using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LibraryHierarchyPopulator : PopulatorBase
    {
        //public const int SAVE_INTERVAL = 1000;

        public readonly object SyncRoot = new object();

        private LibraryHierarchyPopulator(bool reportProgress)
            : base(reportProgress)
        {
            this.Command = new ThreadLocal<LibraryHierarchyPopulatorCommand>(true);
        }

        public LibraryHierarchyPopulator(IDatabaseComponent database, ITransactionSource transaction, bool reportProgress)
            : this(reportProgress)
        {
            this.Database = database;
            this.Transaction = transaction;
        }

        public IDatabaseComponent Database { get; private set; }

        public ITransactionSource Transaction { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        private ThreadLocal<LibraryHierarchyPopulatorCommand> Command { get; set; }

        public override void InitializeComponent(ICore core)
        {
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            base.InitializeComponent(core);
        }

        public void Populate(IDatabaseReader reader)
        {
            if (this.ReportProgress)
            {
                this.Name = "Populating library hierarchies";
                this.Position = 0;
                this.Count = (
                    this.Database.Set<LibraryHierarchyLevel>().Count * this.Database.Set<LibraryItem>().Count
                );
            }

            var interval = Math.Max(Convert.ToInt32(this.Count * 0.01), 1);
            var position = 0;

            Parallel.ForEach(reader, this.ParallelOptions, record =>
            {
                var command = this.GetOrAddCommand();
                command.Parameters["libraryHierarchyId"] = record["LibraryHierarchy_Id"];
                command.Parameters["libraryHierarchyLevelId"] = record["LibraryHierarchyLevel_Id"];
                command.Parameters["libraryItemId"] = record["LibraryItem_Id"];
                command.Parameters["displayValue"] = this.ExecuteScript(command.ScriptingContext, record, "DisplayScript");
                command.Parameters["sortValue"] = this.ExecuteScript(command.ScriptingContext, record, "SortScript");
                command.Parameters["isLeaf"] = record["IsLeaf"];
                command.Command.ExecuteNonQuery();
                //command.Increment();

                if (this.ReportProgress)
                {
                    if (position % interval == 0)
                    {
                        lock (this.SyncRoot)
                        {
                            var fileName = record["FileName"] as string;
                            this.Description = new FileInfo(fileName).Name;
                            this.Position = position;
                        }
                    }
                    Interlocked.Increment(ref position);
                }
            });
        }

        private object ExecuteScript(IScriptingContext scriptingContext, IDatabaseReaderRecord record, string name)
        {
            var script = record[name] as string;
            if (string.IsNullOrEmpty(script))
            {
                return string.Empty;
            }
            var fileName = record["FileName"] as string;
            var metaData = new Dictionary<string, object>();
            for (var a = 0; true; a++)
            {
                var keyName = string.Format("Key_{0}", a);
                if (!record.Contains(keyName))
                {
                    break;
                }
                var key = (record[keyName] as string).ToLower();
                var valueName = string.Format("Value_{0}_Value", a);
                var value = record[valueName] == DBNull.Value ? null : record[valueName];
                metaData.Add(key, value);
            }
            scriptingContext.SetValue("fileName", fileName);
            scriptingContext.SetValue("tag", metaData);
            try
            {
                return scriptingContext.Run(script);
            }
            catch (ScriptingException e)
            {
                return e.Message;
            }
        }

        private LibraryHierarchyPopulatorCommand GetOrAddCommand()
        {
            if (this.Command.IsValueCreated)
            {
                return this.Command.Value;
            }
            return this.Command.Value = new LibraryHierarchyPopulatorCommand(this.Database, this.Transaction, this.ScriptingRuntime);
        }

        protected override void OnDisposing()
        {
            foreach (var command in this.Command.Values)
            {
                command.Dispose();
            }
            this.Command.Dispose();
            base.OnDisposing();
        }

        private class LibraryHierarchyPopulatorCommand : BaseComponent
        {
            public LibraryHierarchyPopulatorCommand(IDatabaseComponent database, ITransactionSource transaction, IScriptingRuntime scriptingRuntime)
            {
                this.Database = database;
                this.Transaction = transaction;
                this.ScriptingContext = scriptingRuntime.CreateContext();
                this.CreateCommand();
            }

            public IDatabaseComponent Database { get; private set; }

            public ITransactionSource Transaction { get; private set; }

            public IScriptingContext ScriptingContext { get; private set; }

            public IDbCommand Command { get; private set; }

            public IDatabaseParameters Parameters { get; private set; }

            //public int Batch { get; private set; }

            public void CreateCommand()
            {
                var parameters = default(IDatabaseParameters);
                this.Command = this.Database.Connection.CreateCommand(
                    this.Database.Queries.AddLibraryHierarchyRecord,
                    out parameters,
                    this.Transaction
                );
                this.Parameters = parameters;
            }

            //public void Increment()
            //{
            //    if (this.Batch++ >= SAVE_INTERVAL)
            //    {
            //        this.Transaction.Commit();
            //        this.Transaction.Bind(this.Command);
            //        this.Batch = 0;
            //    }
            //}

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (this.IsDisposed || !disposing)
                {
                    return;
                }
                this.OnDisposing();
                this.IsDisposed = true;
            }

            protected virtual void OnDisposing()
            {
                this.ScriptingContext.Dispose();
                this.Command.Dispose();
            }

            ~LibraryHierarchyPopulatorCommand()
            {
                Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
                this.Dispose(true);
            }
        }
    }
}
