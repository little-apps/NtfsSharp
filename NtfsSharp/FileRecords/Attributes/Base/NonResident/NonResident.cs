using System;
using NtfsSharp.Helpers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NtfsSharp.Data;

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

        public IEnumerable<Cluster> GetAllDataAsClusters()
        {
            var clusters = new List<Cluster>();

            foreach (var dataBlock in DataBlocks)
            {
                clusters.AddRange(GetDataAsClusters(dataBlock));
            }

            return clusters;
        }

        public byte[] GetAllDataAsBytes()
        {
            var bytes = new List<byte>();

            foreach (var dataBlock in DataBlocks)
            {
                bytes.AddRange(GetDataAsBytes(dataBlock));
            }

            return bytes.ToArray();
        }

        public IEnumerable<Cluster> GetDataAsClusters(DataBlock dataBlock)
        {
            if (!DataBlocks.Contains(dataBlock))
                throw new ArgumentOutOfRangeException(nameof(dataBlock), "Data block is not part of this file record");

            for (ulong i = 0; i < dataBlock.RunLength; i++)
            {
                yield return _volume.ReadLcn(dataBlock.LcnOffset + i);
            }
        }

        public byte[] GetDataAsBytes(DataBlock dataBlock)
        {
            if (!DataBlocks.Contains(dataBlock))
                throw new ArgumentOutOfRangeException(nameof(dataBlock), "Data block is not part of this file record");

            var data = new byte[dataBlock.RunLength * _volume.BytesPerSector * _volume.SectorsPerCluster];

            for (long i = 0, currentLcn = dataBlock.LcnOffset; i < dataBlock.RunLength; i++, currentLcn++)
            {
                var cluster = _volume.ReadLcn((ulong)currentLcn);

                Array.Copy(cluster.Data, 0, data, i * _volume.BytesPerSector, cluster.Data.Length);
            }

            return data;
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
