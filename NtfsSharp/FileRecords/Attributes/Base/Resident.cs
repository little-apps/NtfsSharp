using NtfsSharp.Helpers;
using System.Runtime.InteropServices;

namespace NtfsSharp.FileRecords.Attributes.Base
{
    public sealed class Resident : AttributeHeader
    {
        public new static uint HeaderSize => (uint)Marshal.SizeOf<ResidentAttribute>();

        public ResidentAttribute SubHeader { get; private set; }

        public Resident(NTFS_ATTRIBUTE_HEADER header, byte[] data) : base(header, data)
        {
            SubHeader = data.ToStructure<ResidentAttribute>(CurrentOffset);
            CurrentOffset += HeaderSize;

            ReadName();
        }
        
        public struct ResidentAttribute
        {
            public readonly uint AttributeLength;
            public readonly ushort AttributeOffset;
            public readonly byte IndexedFlag;
            public readonly byte Padding;
        }
    }
}
