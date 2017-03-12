using NtfsSharp.Helpers;
using System.Runtime.InteropServices;
using System;

namespace NtfsSharp.FileRecords.Attributes.Base
{
    public sealed class Resident : AttributeHeader
    {
        public new static uint HeaderSize => (uint)Marshal.SizeOf<ResidentAttribute>();

        public ResidentAttribute SubHeader { get; private set; }

        public Resident(NTFS_ATTRIBUTE_HEADER header, byte[] data, FileRecord fileRecord) : base(header, data, fileRecord)
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

        public override byte[] ReadBody()
        {
            var body = new byte[Bytes.Length - CurrentOffset];

            Array.Copy(Bytes, CurrentOffset, body, 0, body.Length);

            return body;
        }
    }
}
