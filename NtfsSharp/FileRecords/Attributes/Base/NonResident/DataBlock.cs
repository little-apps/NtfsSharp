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

        public bool LcnOffsetNegative => LcnOffset >> ((OffsetFieldLength - 1) * 8) >= 0x80;

        public readonly ulong StartVcn;
        public ulong LastVcn => StartVcn + RunLength - 1;

        private DataBlock(ushort lengthFieldLength, ushort offsetFieldLength, uint runLength, uint lcnOffset, ulong startVcn)
        {
            LengthFieldLength = lengthFieldLength;
            OffsetFieldLength = offsetFieldLength;
            RunLength = runLength;
            LcnOffset = lcnOffset;
            StartVcn = startVcn;
        }

        public static DataBlock GetDataBlockFromRun(byte[] data, ref uint offset, ulong startVcn)
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

            return new DataBlock(lengthBytes, offsetBytes, runLength, runOffset, startVcn);
        }
    }
}
