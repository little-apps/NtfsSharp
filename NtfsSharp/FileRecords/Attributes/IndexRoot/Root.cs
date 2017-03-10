using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NtfsSharp.Helpers;
using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.FileRecords.Attributes.IndexRoot
{
    public class Root : AttributeBodyBase
    {
        public new static uint HeaderSize => (uint)Marshal.SizeOf<NTFS_ATTR_INDEX_ROOT>();

        public NTFS_ATTR_INDEX_ROOT Data { get; private set; }

        public readonly List<FileName> FileNameEntries = new List<FileName>();

        public Root(AttributeHeader header) : base(header)
        {
            Data = Bytes.ToStructure<NTFS_ATTR_INDEX_ROOT>(CurrentOffset);
            CurrentOffset += HeaderSize;

            var shouldContinue = true;

            while (shouldContinue)
            {
                var fileName = new FileName(Bytes, CurrentOffset);
                FileNameEntries.Add(fileName);
                CurrentOffset += fileName.Header.IndexEntryLength;

                shouldContinue = !fileName.Header.Flags.HasFlag(FileName.Flags.IsLastEntry) &&
                                 fileName.Header.IndexEntryLength > 0 && CurrentOffset < CurrentOffset + Header.Header.Length;
            }
        }

        [Flags]
        public enum Flags : uint
        {
            HasLargeIndex = 1 << 0
        }

        public struct NTFS_ATTR_INDEX_ROOT
        {
            public readonly uint AttributeType;
            public readonly uint CollationRule;
            public readonly uint IndexAllocationEntrySize;
            public readonly uint ClustersPerIndexRecord;
            public readonly uint FirstIndexEntryOffset;
            public readonly uint IndexEntriesSize;
            public readonly uint IndexEntriesAllocated;
            public readonly Flags Flags;
        }
    }
}
