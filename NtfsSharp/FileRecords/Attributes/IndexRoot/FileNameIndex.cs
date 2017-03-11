using System;
using System.Runtime.InteropServices;
using NtfsSharp.FileRecords.Attributes.Shared;
using NtfsSharp.Helpers;
using NtfsSharp.PInvoke;
using static NtfsSharp.PInvoke.Structs;

namespace NtfsSharp.FileRecords.Attributes.IndexRoot
{
    public class FileNameIndex
    {
        public static uint HeaderSize => (uint)Marshal.SizeOf<NTFS_ATTR_INDEX_ENTRY_HEADER>();
        public readonly NTFS_ATTR_INDEX_ENTRY_HEADER Header;
        public readonly byte[] Stream;
        public readonly ulong SubNode;
        public readonly FileName FileName;
        
        public FileNameIndex(byte[] data, uint currentOffset)
        {
            Header = data.ToStructure<NTFS_ATTR_INDEX_ENTRY_HEADER>(currentOffset);

            if (!Header.Flags.HasFlag(Enums.IndexEntryFlags.IsLastEntry) && Header.StreamLength > 0)
            {
                Stream = new byte[Header.StreamLength];
                Array.Copy(data, currentOffset + HeaderSize, Stream, 0, Stream.Length);
                FileName = new FileName(Stream);
            }

            if (Header.Flags.HasFlag(Enums.IndexEntryFlags.HasSubNode))
                SubNode = BitConverter.ToUInt64(data, (int) (currentOffset + Header.IndexEntryLength - 8));
        }

        
    }
}
