using System;
using NtfsSharp.Helpers;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NtfsSharp.FileRecords.Attributes.Base.NonResident
{
    public sealed class NonResident : AttributeHeader
    {
        public new static uint HeaderSize => (uint)Marshal.SizeOf<NonResidentAttribute>();

        public NonResidentAttribute SubHeader { get; private set; }

        public List<DataBlock> DataBlocks = new List<DataBlock>();

        private readonly Volume _volume;

        public NonResident(NTFS_ATTRIBUTE_HEADER header, byte[] data, Volume volume) : base(header, data)
        {
            if (volume == null)
                throw new ArgumentNullException(nameof(volume));

            _volume = volume;

            SubHeader = data.ToStructure<NonResidentAttribute>(CurrentOffset);
            CurrentOffset += HeaderSize;

            ReadName();
            ReadDataBlocks(data);
        }

        private void ReadDataBlocks(byte[] data)
        {
            var currentOffset = CurrentOffset;

            while (currentOffset < Header.Length && data[currentOffset] != 0)
            {
                DataBlocks.Add(DataBlock.GetDataBlockFromRun(data, ref currentOffset));
            }

            CurrentOffset = currentOffset;
        }

        public struct NonResidentAttribute
        {
            public readonly ulong StartingVCN;
            public readonly ulong LastVCN;
            public readonly ushort DataRunsOffset;
            public readonly ushort CompressionUnitSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly byte[] Padding;
            public readonly ulong AttributeAllocated;
            public readonly ulong AttributeSize;
            public readonly ulong StreamDataSize;
        }
    }
}
