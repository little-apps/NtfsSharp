using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NtfsSharp.Tests.Driver.Attributes.NonResident
{
    public abstract class NonResidentAttributeBase : DummyAttributeBase
    {
        public NonResidentAttribute BodyHeader;

        protected NonResidentAttributeBase()
        {
            Header.NonResident = true;
        }

        public override byte[] GetData()
        {
            var dataRunsOffset = BodyHeader.DataRunsOffset;
            if (dataRunsOffset == 0)
                dataRunsOffset = (ushort) (HeaderLength + Marshal.SizeOf<NonResidentAttribute>());
            
            var body = GenerateDataBlockData(dataRunsOffset);

            // Set attributes for body header if not set
            if (BodyHeader.LastVCN == 0)
                BodyHeader.LastVCN = BodyHeader.StartingVCN + (ulong) AdditionalClusters.Count;

            if (BodyHeader.AttributeAllocated == 0)
                BodyHeader.AttributeAllocated = (ulong) (AdditionalClusters.Count * DummyDriver.BytesPerSector *
                                                         DummyDriver.SectorsPerCluster);

            // If AttributeSize and StreamDataSize are 0 -> use same value as AttributeAllocated
            if (BodyHeader.AttributeSize == 0)
                BodyHeader.AttributeSize = BodyHeader.AttributeAllocated;

            if (BodyHeader.StreamDataSize == 0)
                BodyHeader.StreamDataSize = BodyHeader.AttributeAllocated;

            // Get body header as bytes (now that default values have been taken care of)
            var bodyHeaderBytes = StructureToBytes(BodyHeader);

            // Set bodyLength to [Size of body header] + [Size of body]
            var bodyLength = (uint) (bodyHeaderBytes.Length + body.Length);

            // Get header bytes now that BodyLength is set
            var header = GetHeaderBytes(bodyLength);
            
            var bytes = new byte[header.Length + bodyHeaderBytes.Length + body.Length];
            Array.Copy(header, 0, bytes, 0, header.Length);
            Array.Copy(bodyHeaderBytes, 0, bytes, header.Length, bodyHeaderBytes.Length);
            Array.Copy(body, 0, bytes, header.Length + bodyHeaderBytes.Length, body.Length);

            return bytes;
        }

        protected byte[] GenerateDataBlockData(ushort dataRunOffset)
        {
            var bytes = new List<byte>();

            DataRun currentDataRun = null;
            var currentVcn = BodyHeader.StartingVCN;
            ulong lastLcnOffset = 0, lastLcn = 0;

            // Determine data runs from additional clusters
            foreach (var additionalCluster in AdditionalClusters)
            {
                if (BodyHeader.LastVCN > 0 && currentVcn > BodyHeader.LastVCN)
                    throw new IndexOutOfRangeException("The last VCN is smaller than the number of VCNs");

                if (currentDataRun == null || lastLcn + 1 != additionalCluster.Key)
                {
                    if (currentDataRun != null)
                    {
                        // Write current data run
                        var dataRunBytes = currentDataRun.GetDataRunBytes();
                        bytes.AddRange(dataRunBytes);
                        
                    }
                    
                    // Cluster is not contigious, create another datablock
                    currentDataRun = new DataRun(1, additionalCluster.Key - lastLcnOffset);
                    lastLcnOffset = additionalCluster.Key;
                }
                else
                {
                    // Increment data run length
                    currentDataRun.Length++;
                }

                lastLcn = additionalCluster.Key;
                currentVcn++;
            }

            // Add last data run
            if (currentDataRun != null)
            {
                var dataRunBytes = currentDataRun.GetDataRunBytes();
                bytes.AddRange(dataRunBytes);
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Splits data into chunks of 4096 bytes (size of a cluster) and adds them to the additional clusters.
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="startLcn">Logical cluster number to start at. (default: 100)</param>
        /// <returns>Array of LCNs for each added cluster</returns>
        /// <remarks>The virtual cluster numbers is each clusters index + 1</remarks>
        public ulong[] AddDataAsVirtualClusters(byte[] data, ulong startLcn = 100)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null.");

            if (data.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(data), "Data length cannot be 0.");

            const int bytesPerCluster = (int) (DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster);
            var lcn = GetNextLcn(startLcn == 0 ? 100 : startLcn);
            
            // Split data into chunks of 4096 bytes
            var clusters = (uint) Math.Ceiling((double) data.Length / bytesPerCluster);
            var dataRemaining = data.Length;

            var addedLcns = new ulong[clusters];

            for (var i = 0; i < clusters; i++)
            {
                var startOffset = i * bytesPerCluster;
                var chunkLength = dataRemaining >= bytesPerCluster ? bytesPerCluster : dataRemaining;

                var dataCluster = new DataCluster();

                Array.Copy(data, startOffset, dataCluster.Data, 0, chunkLength);

                AdditionalClusters.Add(lcn, dataCluster);
                addedLcns[i] = lcn;

                lcn = GetNextLcn(lcn);
                dataRemaining -= bytesPerCluster;
            }

            return addedLcns;
        }

        /// <summary>
        /// Gets the next available logical cluster number
        /// </summary>
        /// <param name="lcn">LCN to look at first</param>
        /// <returns>Next available LCN</returns>
        /// <remarks>This does not check any clusters added to the <see cref="Volume"/> directly</remarks>
        private ulong GetNextLcn(ulong lcn)
        {
            var i = lcn;
            while (AdditionalClusters.ContainsKey(i++)) ;

            return i;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NonResidentAttribute
        {
            public ulong StartingVCN;
            public ulong LastVCN;
            public ushort DataRunsOffset;
            public readonly ushort CompressionUnitSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Padding;
            public ulong AttributeAllocated;
            public ulong AttributeSize;
            public ulong StreamDataSize;
        }

        
    }
}
