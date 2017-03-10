using System;
using System.Runtime.InteropServices;
using NtfsSharp.Helpers;
using NtfsSharp.PInvoke;

namespace NtfsSharp.FileRecords.Attributes.IndexRoot
{
    public class FileName
    {
        public new static uint HeaderSize => (uint)Marshal.SizeOf<NTFS_ATTR_INDEX_ENTRY_HEADER>();
        public readonly NTFS_ATTR_INDEX_ENTRY_HEADER Header;
        public readonly byte[] Stream;
        public readonly ulong SubNode;
        
        public FileName(byte[] data, uint currentOffset)
        {
            Header = data.ToStructure<NTFS_ATTR_INDEX_ENTRY_HEADER>(currentOffset);

            if (!Header.Flags.HasFlag(Flags.IsLastEntry) && Header.StreamLength > 0)
            {
                Stream = new byte[Header.StreamLength];
                Array.Copy(data, currentOffset + HeaderSize, Stream, 0, Stream.Length);
            }

            if (Header.Flags.HasFlag(Flags.HasSubNode))
                SubNode = BitConverter.ToUInt64(data, (int) (currentOffset + Header.IndexEntryLength - 8));
        }

        [Flags]
        public enum Flags : byte
        {
            HasSubNode = 1 << 0,
            IsLastEntry = 1 << 1
        }

        public struct NTFS_ATTR_INDEX_ENTRY_HEADER
        {
            public readonly Structs.FILE_REFERENCE FileReference;
            public readonly ushort IndexEntryLength;
            public readonly ushort StreamLength;
            public readonly Flags Flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] Padding;
        }
    }
}
