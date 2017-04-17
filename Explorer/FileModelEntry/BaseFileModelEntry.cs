using System;
using System.Collections.Generic;
using NtfsSharp.FileRecords;

namespace Explorer.FileModelEntry
{
    public abstract class BaseFileModelEntry : IEquatable<BaseFileModelEntry>
    {
        public abstract ulong FileRecordNum { get; }
        public abstract FileRecord FileRecord { get; }

        public abstract string FilePath { get; }

        public abstract string Filename { get; }
        public abstract string DateModified { get; }
        public abstract string ActualSize { get; }
        public abstract string AllocatedSize { get; }
        public abstract string Attributes { get; }

        public static string SizeToString(ulong size)
        {
            double len = size;

            var sizes = new[] { "B", "KB", "MB", "GB", "TB" };
            var order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return $"{len:0.##} {sizes[order]}";
        }

        public bool Equals(BaseFileModelEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return FileRecordNum == other.FileRecordNum;
        }
    }

    public class FileModelEntryByRecordNumComparer : IComparer<BaseFileModelEntry>
    {
        public int Compare(BaseFileModelEntry x, BaseFileModelEntry y)
        {
            if (x == null)
                return -1;

            if (y == null)
                return 1;

            return (int)(x.FileRecordNum - y.FileRecordNum);
        }
    }
}
