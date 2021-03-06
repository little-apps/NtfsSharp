﻿using System;
using System.Runtime.InteropServices;
using NtfsSharp.Helpers;

namespace NtfsSharp.Files.Attributes.Base
{
    /// <summary>
    /// Resident data is data that is able to fit in the FILE record itself
    /// </summary>
    public sealed class Resident : AttributeHeaderBase
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
            var body = new byte[SubHeader.AttributeLength];

            Array.Copy(Bytes, CurrentOffset, body, 0, body.Length);

            return body;
        }
    }
}
