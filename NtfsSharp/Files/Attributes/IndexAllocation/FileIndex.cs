﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NtfsSharp.Exceptions;
using NtfsSharp.Factories.FileRecords;
using NtfsSharp.Helpers;
using NtfsSharp.PInvoke;

namespace NtfsSharp.Files.Attributes.IndexAllocation
{
    /// <summary>
    /// Represents a file index inside a $INDEX_ALLOCATION attribute
    /// </summary>
    public class FileIndex
    {
        public uint CurrentOffset { get; private set; } = 0;

        public readonly NTFS_INDEX_HEADER Header;

        public readonly ushort[] UpdateSequenceArray;

        public readonly List<FileNameEntry> FileNameEntries = new List<FileNameEntry>();

        /// <summary>
        /// Builds a list of <see cref="FileNameEntries"/>
        /// </summary>
        /// <param name="data">Data containing file index</param>
        public FileIndex(byte[] data, IndexAllocation indexAllocation)
        {
            Header = data.ToStructure<NTFS_INDEX_HEADER>();
            CurrentOffset += (uint) Marshal.SizeOf<NTFS_INDEX_HEADER>();

            if (Header.UpdateSequenceSize > 0)
            {
                try
                {
                    var fixupable = FixupableFactory.Build(data, indexAllocation.Header.FileRecord.Volume.BytesPerSector);
                    fixupable.Fixup(data);
                }
                catch (InvalidEndTagsException ex)
                {
                    throw new InvalidIndexAllocationException(this, $"Fixup could not be performed on sector {ex.InvalidSector} in FileIndex");
                }
            }

            if (Header.Magic.SequenceEqual(new byte[] {0x49, 0x4E, 0x44, 0x58}))
            {
                CurrentOffset = 0x18 + Header.IndexEntriesOffset;

                var shouldContinue = true;

                while (shouldContinue)
                {
                    var fileNameEntry = new FileNameEntry(data, CurrentOffset);

                    if (!fileNameEntry.Header.Flags.HasFlag(Enums.IndexEntryFlags.IsLastEntry))
                        FileNameEntries.Add(fileNameEntry);

                    CurrentOffset = fileNameEntry.CurrentOffset;

                    shouldContinue = !fileNameEntry.Header.Flags.HasFlag(Enums.IndexEntryFlags.IsLastEntry) &&
                                     fileNameEntry.Header.IndexEntryLength > 0 &&
                                     CurrentOffset < Header.IndexEntriesSize + Header.IndexEntriesOffset;
                }

            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct NTFS_INDEX_HEADER
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly byte[] Magic;
            public readonly ushort UpdateSequenceOffset;
            public readonly ushort UpdateSequenceSize;
            public readonly ulong LogFileSequence;
            public readonly ulong VCN; // Virtual Cluster Number
            public readonly uint IndexEntriesOffset;
            public readonly uint IndexEntriesSize;
            public readonly uint IndexEntriesAllocated;
            public readonly uint HasChildren;
            public readonly ushort UpdateSequence;
        }
    }
}
