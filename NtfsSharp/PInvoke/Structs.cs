using System.Runtime.InteropServices;

namespace NtfsSharp.PInvoke
{
    public static class Structs
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct FILE_REFERENCE
        {
            [FieldOffset(0)]
            public ulong Data;
            
            public ulong FileRecordNumber => Data & 0xFFFFFFFFFFFF;
            public ushort SequenceNumber => (ushort) (Data & 0xFFFF000000000000);
        }

        public struct NTFS_ATTR_INDEX_ENTRY_HEADER
        {
            public readonly FILE_REFERENCE FileReference;
            public readonly ushort IndexEntryLength;
            public readonly ushort StreamLength;
            public readonly Enums.IndexEntryFlags Flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] Padding;
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct Nibble
        {
            public byte Value;

            public byte Low => (byte)(Value & 0x0F);
            public byte High => (byte)((Value & 0xF0) >> 4);
        }
    }
}
