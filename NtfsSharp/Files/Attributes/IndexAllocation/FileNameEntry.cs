﻿using System;
using System.Runtime.InteropServices;
using NtfsSharp.Files.Attributes.Shared;
using NtfsSharp.Helpers;
using NtfsSharp.PInvoke;

namespace NtfsSharp.Files.Attributes.IndexAllocation
{
    /// <summary>
    /// Represents a file name entry inside a file index inside a $INDEX_ALLOCATION
    /// </summary>
    public class FileNameEntry
    {
        public uint CurrentOffset { get; private set; }

        public Structs.NTFS_ATTR_INDEX_ENTRY_HEADER Header;
        public readonly FileName FileName;
        public readonly ulong SubNode;

        public FileNameEntry(byte[] data, uint currentOffset)
        {
            var startOffset = currentOffset;

            Header = data.ToStructure<Structs.NTFS_ATTR_INDEX_ENTRY_HEADER>(currentOffset);
            currentOffset += (uint)Marshal.SizeOf<Structs.NTFS_ATTR_INDEX_ENTRY_HEADER>();

            if (!Header.Flags.HasFlag(Enums.IndexEntryFlags.IsLastEntry) && Header.StreamLength > 0)
                FileName = new FileName(data, currentOffset);

            if (Header.Flags.HasFlag(Enums.IndexEntryFlags.HasSubNode))
                SubNode = BitConverter.ToUInt64(data, (int)(startOffset + Header.IndexEntryLength - 8));

            CurrentOffset = startOffset + Header.IndexEntryLength;
        }

    }
}
