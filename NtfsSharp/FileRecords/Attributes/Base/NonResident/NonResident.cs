using System;
using NtfsSharp.Helpers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NtfsSharp.Data;

namespace NtfsSharp.FileRecords.Attributes.Base.NonResident
{
    /// <summary>
    /// Non-resident data is data that is contained on one or more other sectors on the disk
    /// </summary>
    public sealed class NonResident : AttributeHeader
    {
        public new static uint HeaderSize => (uint)Marshal.SizeOf<NonResidentAttribute>();

        public NonResidentAttribute SubHeader { get; private set; }

        /// <summary>
        /// A list of the data blocks or data runs to locate the data on the disk
        /// </summary>
        public List<DataBlock> DataBlocks = new List<DataBlock>();

        public NonResident(NTFS_ATTRIBUTE_HEADER header, byte[] data, FileRecord fileRecord) : base(header, data, fileRecord)
        {
            SubHeader = data.ToStructure<NonResidentAttribute>(CurrentOffset);
            CurrentOffset += HeaderSize;

            ReadName();
            ReadDataBlocks(data);
        }

        /// <summary>
        /// Converts the data runs to <see cref="DataBlock"/>
        /// </summary>
        /// <param name="data"></param>
        private void ReadDataBlocks(byte[] data)
        {
            var currentOffset = CurrentOffset;

            ulong vcn = 0;

            while (currentOffset < Header.Length && data[currentOffset] != 0)
            {
                var dataBlock = DataBlock.GetDataBlockFromRun(data, ref currentOffset, vcn);

                if (dataBlock.LastVcn > SubHeader.LastVCN - SubHeader.StartingVCN)
                {
                    DataBlocks.Clear();
                    return;
                }

                DataBlocks.Add(dataBlock);

                vcn += dataBlock.RunLength;
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

        /// <summary>
        /// Gets non-resident data as clusters
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Cluster> GetAllDataAsClusters()
        {
            var clusters = new List<Cluster>();

            foreach (var dataBlock in DataBlocks)
            {
                clusters.AddRange(GetDataAsClusters(dataBlock));
            }

            return clusters;
        }

        /// <summary>
        /// Gets non-resident data as byte array
        /// </summary>
        /// <returns></returns>
        public byte[] GetAllDataAsBytes()
        {
            var bytes = new List<byte>();
            ulong vcn = 0;
            var lengthLeft = SubHeader.AttributeSize;

            foreach (var dataBlock in DataBlocks)
            {
                if (dataBlock.LcnOffset == 0xFFFFFFFF)
                    continue;

                var dataBlockLcn = VcnToLcn(vcn);

                if (!dataBlockLcn.HasValue)
                    break;
                
                vcn += dataBlock.RunLength;
                
                var blockSize = dataBlock.RunLength * FileRecord.Volume.BytesPerSector * FileRecord.Volume.SectorsPerCluster;

                FileRecord.Volume.Disk.Move(dataBlockLcn.Value * FileRecord.Volume.BytesPerSector *
                                            FileRecord.Volume.SectorsPerCluster);
                

                var blockData = FileRecord.Volume.Disk.SafeReadFile((uint) (lengthLeft >= blockSize ? blockSize : lengthLeft));
                bytes.AddRange(blockData);

                lengthLeft -= dataBlock.RunLength * FileRecord.Volume.BytesPerSector * FileRecord.Volume.SectorsPerCluster;
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Uses data block to get it's cluster(s)
        /// </summary>
        /// <param name="dataBlock">DataBlock instance</param>
        /// <returns>List of <see cref="Cluster"/> that data block has</returns>
        public IEnumerable<Cluster> GetDataAsClusters(DataBlock dataBlock)
        {
            if (!DataBlocks.Contains(dataBlock))
                throw new ArgumentOutOfRangeException(nameof(dataBlock), "Data block is not part of this file record");

            for (ulong i = 0; i < dataBlock.RunLength; i++)
            {
                yield return FileRecord.Volume.ReadLcn(dataBlock.LcnOffset + i);
            }
        }

        /// <summary>
        /// Uses data block to get it's bytes
        /// </summary>
        /// <param name="dataBlock">DataBlock instance</param>
        /// <param name="clustersToRead">Number of clusters to read. If 0, all clusters in data run are read. (default: 0)</param>
        /// <param name="startVcn">Starting VCN to read from (default: 0)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if data block is not part of this attribute or clustersToRead is greater than data run length</exception>
        /// <returns>Bytes that data block has</returns>
        public byte[] GetDataAsBytes(DataBlock dataBlock, uint clustersToRead = 0, uint startVcn = 0)
        {
            if (!DataBlocks.Contains(dataBlock))
                throw new ArgumentOutOfRangeException(nameof(dataBlock), "Data block is not part of this file record");

            if (clustersToRead > dataBlock.RunLength)
                throw new ArgumentOutOfRangeException(nameof(clustersToRead),
                    "Number of clusters to read exceeds what is in datablock");

            if (clustersToRead == 0)
                clustersToRead = dataBlock.RunLength;

            var data = new byte[clustersToRead * FileRecord.Volume.BytesPerSector * FileRecord.Volume.SectorsPerCluster];

            var lcnOffset = VcnToLcn(dataBlock.StartVcn + startVcn);

            if (!lcnOffset.HasValue)
                return new byte[0];

            for (long i = 0, currentLcn = (long) lcnOffset.Value; i < clustersToRead; i++, currentLcn++)
            {
                var cluster = FileRecord.Volume.ReadLcn((ulong) currentLcn);

                Array.Copy(cluster.Data, 0, data, i * FileRecord.Volume.BytesPerSector * FileRecord.Volume.SectorsPerCluster, cluster.Data.Length);
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
