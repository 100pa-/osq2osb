﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osq2osb {
    class FileCollectionWatcher {
        private IDictionary<string, FileSystemWatcher> watchers = new Dictionary<string, FileSystemWatcher>();

        public IEnumerable<string> Files {
            get {
                return watchers.Keys;
            }
        }

        public FileCollectionWatcher() {
        }

        public void Add(string file) {
            var watcher = new FileSystemWatcher(Path.GetDirectoryName(file), Path.GetFileName(file));

            watcher.Changed += this.FileChanged;
            watcher.EnableRaisingEvents = true;

            watchers[file] = watcher;
        }

        public void Clear() {
            foreach(var p in watchers) {
                p.Value.Dispose();
            }

            watchers.Clear();
        }

        public event FileSystemEventHandler Changed;

        protected virtual void OnChanged(FileSystemEventArgs e) {
            FileSystemEventHandler changed = Changed;

            if(changed != null) {
                changed(this, e);
            }
        }

        private void FileChanged(object sender, FileSystemEventArgs e) {
            OnChanged(e);
        }
    }
}