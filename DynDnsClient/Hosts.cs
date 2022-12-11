using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DynDnsClient
{
    internal class Hosts : IDisposable
    {
        private const string FileName = "Hosts.txt";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly FileSystemWatcher hostsWatcher;

        private EventHandler changedHandler;

        internal Hosts()
        {
            hostsWatcher = new FileSystemWatcher(Directories.CurrentDirectory, FileName);
            hostsWatcher.Created += (sender, e) => FireChanged();
            hostsWatcher.Deleted += (sender, e) => FireChanged();
            hostsWatcher.Changed += (sender, e) => FireChanged();
            hostsWatcher.Renamed += (sender, e) => FireChanged();
            hostsWatcher.EnableRaisingEvents = true;
            hostsWatcher.NotifyFilter = NotifyFilters.Size;

            Log.InfoFormat("Watching hosts file '{0}' in directory '{1}' for changes", FileName, Directories.CurrentDirectory);
        }

        internal event EventHandler Changed
        {
            add { changedHandler += value; }
            remove { changedHandler -= value; }
        }

        internal string[] Read()
        {
            if (!File.Exists(FilePath))
            {
                Log.WarnFormat("Either file '{0}' wasn't found or no hosts exists in the file", FilePath);
                return new string[0];
            }

            return ReadAllLines()
                .Select(host => host.Trim())
                .Where(host => !string.IsNullOrWhiteSpace(host))
                .Where(host => !host.StartsWith("#"))
                .ToArray();
        }

        private IEnumerable<string> ReadAllLines()
        {
            using (var stream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine();
                }
        }

        private void FireChanged()
        {
            EventHandler handler = changedHandler;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private static string FilePath
        {
            get { return Path.Combine(Directories.CurrentDirectory, FileName); }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (hostsWatcher != null)
                {
                    hostsWatcher.Dispose();
                }
            }
        }

        #endregion
    }
}