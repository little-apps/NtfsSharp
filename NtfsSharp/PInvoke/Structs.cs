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
            public ulong SequenceNumber => Data & 0xFFFF000000000000;
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
