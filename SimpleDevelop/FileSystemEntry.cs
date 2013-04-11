using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleDevelop
{
    public class FileSystemEntry
    {
        FileSystemInfo fileSystemInfo;

        static readonly FileSystemEntry[] NoEntries = new FileSystemEntry[0];

        public FileSystemEntry(string location)
        {
            if (Directory.Exists(location))
            {
                Initialize(new DirectoryInfo(location));
            }
            else if (File.Exists(location))
            {
                Initialize(new FileInfo(location));
                Entries = NoEntries;
            }
            else
            {
                throw new ArgumentException("The location '{0}' doesn't exist.".F(location), "location");
            }
        }

        public FileSystemEntry(FileSystemInfo info)
        {
            Initialize(info);
        }

        public string Name
        {
            get { return this.fileSystemInfo.Name; }
        }

        public string Path
        {
            get { return this.fileSystemInfo.FullName; }
        }

        public string Type
        {
            get { return this.fileSystemInfo is DirectoryInfo ? "folder" : "file"; }
        }

        public FileSystemEntry[] Entries { get; private set; }

        void Initialize(FileSystemInfo info)
        {
            this.fileSystemInfo = info;
            FetchChildEntries();
        }

        void FetchChildEntries()
        {
            if (this.fileSystemInfo is DirectoryInfo && Entries == null)
            {
                var children = new List<FileSystemEntry>();
                children.AddRange(((DirectoryInfo)this.fileSystemInfo).GetDirectories().Select(d => new FileSystemEntry(d)));
                children.AddRange(((DirectoryInfo)this.fileSystemInfo).GetFiles("*.cs").Select(f => new FileSystemEntry(f)));

                Entries = children.Count > 0 ? children.ToArray() : NoEntries;
            }
        }
    }
}
