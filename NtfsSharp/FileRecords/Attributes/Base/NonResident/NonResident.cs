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

        public NonResident(NTFS_ATTRIBUTE_HEADER header, byte[] data, FileRecord fileRecord) : base(header, data, fileRecord)
        {
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

        /// <summary>
        /// Converts a VCN (Virtual Cluster Number) to LCN (Logical Cluster Number)
        /// </summary>
        /// <param name="vcn">Virtual Cluster Number</param>
        /// <returns>LCN or null if it wasn't found</returns>
        public ulong? VcnToLcn(ulong vcn)
        {
            ulong lcnOffset = 0;

            var signExtends = new ulong[]
            {
                0xffffffffffffff00,
                0xffffffffffff0000,
                0xffffffffff000000,
                0xffffffff00000000,
                0xffffff0000000000,
                0xffff000000000000,
                0xff00000000000000,
                0x0000000000000000
            };

            foreach (var dataBlock in DataBlocks)
            {
                if (dataBlock.LcnOffsetNegative)
                {
                    // Last bit in last byte is 1 (meaning it's negative)
                    lcnOffset += dataBlock.LcnOffset + signExtends[dataBlock.OffsetFieldLength - 1];
                }
                else
                {
                    // Offset is positive
                    lcnOffset += dataBlock.LcnOffset;
                }

                // Is VCN in this run?
                if (vcn < dataBlock.RunLength)
                    return vcn + lcnOffset;

                vcn -= dataBlock.RunLength;
            }

            // Not found
            return null;
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
                yield return FileRecord.Volume.ReadLcn(dataBlock.LcnOffset + i);
            }
        }

        public byte[] GetDataAsBytes(DataBlock dataBlock)
        {
            if (!DataBlocks.Contains(dataBlock))
                throw new ArgumentOutOfRangeException(nameof(dataBlock), "Data block is not part of this file record");

            var data = new byte[dataBlock.RunLength * FileRecord.Volume.BytesPerSector * FileRecord.Volume.SectorsPerCluster];

            for (long i = 0, currentLcn = dataBlock.LcnOffset; i < dataBlock.RunLength; i++, currentLcn++)
            {
                var cluster = FileRecord.Volume.ReadLcn((ulong)currentLcn);

                Array.Copy(cluster.Data, 0, data, i * FileRecord.Volume.BytesPerSector, cluster.Data.Length);
            }

            return data;
        }

        public override byte[] ReadBody()
        {
            return GetAllDataAsBytes();
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
