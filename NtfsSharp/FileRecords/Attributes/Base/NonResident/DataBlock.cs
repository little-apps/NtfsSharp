using NtfsSharp.Data;
using System;
using System.Collections.Generic;
using static NtfsSharp.PInvoke.Structs;

namespace NtfsSharp.FileRecords.Attributes.Base.NonResident
{
    public class DataBlock
    {
        public readonly ushort LengthFieldLength;
        public readonly ushort OffsetFieldLength;

        /// <summary>
        /// The number of clusters from the LCN
        /// </summary>
        public readonly uint RunLength;

        /// <summary>
        /// LCN of run
        /// </summary>
        public readonly uint LcnOffset;

        private DataBlock(ushort lengthFieldLength, ushort offsetFieldLength, uint runLength, uint lcnOffset)
        {
            LengthFieldLength = lengthFieldLength;
            OffsetFieldLength = offsetFieldLength;
            RunLength = runLength;
            LcnOffset = lcnOffset;
        }

        public IEnumerable<Cluster> GetDataAsClusters(Volume vol)
        {
            for (ulong i = 0; i < RunLength; i++)
            {
                yield return vol.ReadLcn(LcnOffset + i);
            }
        }

        public byte[] GetDataAsBytes(Volume vol)
        {
            var data = new byte[RunLength * vol.BytesPerSector * vol.SectorsPerCluster];
            
            for (long i = 0, currentLcn = LcnOffset; i < RunLength; i++, currentLcn++)
            {
                var cluster = vol.ReadLcn((ulong) currentLcn);

                Array.Copy(cluster.Data, 0, data, i * vol.BytesPerSector, cluster.Data.Length);
            }

            return data;
        }

        public static DataBlock GetDataBlockFromRun(byte[] data, ref uint offset)
        {
            var lengthOffsetFieldSize = new Nibble { Value = data[offset] };

            var lengthBytes = lengthOffsetFieldSize.Low;
            var offsetBytes = lengthOffsetFieldSize.High;

            offset++;

            uint runLength = 0;

            for (var i = 0; i < lengthBytes; i++)
            {
                runLength |= (uint)data[offset + i] << (i * 8);
            }

            offset += lengthBytes;

            uint runOffset = 0;

            for (var i = 0; i < offsetBytes; i++)
            {
                runOffset |= (uint)data[offset + i] << (i * 8);
            }

            offset += offsetBytes;

            return new DataBlock(lengthBytes, offsetBytes, runLength, runOffset);
        }
    }
}
