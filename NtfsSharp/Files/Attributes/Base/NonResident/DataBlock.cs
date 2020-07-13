using static NtfsSharp.PInvoke.Structs;

namespace NtfsSharp.Files.Attributes.Base.NonResident
{
    public class DataBlock
    {
        private static readonly ulong[] SignedExtends = {
            0xffffffffffffff00,
            0xffffffffffff0000,
            0xffffffffff000000,
            0xffffffff00000000,
            0xffffff0000000000,
            0xffff000000000000,
            0xff00000000000000,
            0x0000000000000000
        };

        internal readonly ushort LengthFieldLength;
        internal readonly ushort OffsetFieldLength;

        /// <summary>
        /// The number of clusters from the LCN
        /// </summary>
        public readonly uint RunLength;

        /// <summary>
        /// LCN of run
        /// </summary>
        /// <remarks>This is the raw value. Use <seealso cref="ActualLcnOffset"/> for the actual offset.</remarks>
        public readonly ulong LcnOffset;

        /// <summary>
        /// Gets the LCN signed (could be negative) 
        /// </summary>
        public long SignedLcnOffset => (long) LcnOffset + (LcnOffsetNegative ? (long) SignedExtends[OffsetFieldLength - 1] : 0);

        /// <summary>
        /// The actual LCN (taking into account all the other data blocks)
        /// </summary>
        public ulong ActualLcnOffset { get; internal set; }

        /// <summary>
        /// Is the LCN negative?
        /// </summary>
        public bool LcnOffsetNegative => LcnOffset >> ((OffsetFieldLength - 1) * 8) >= 0x80;

        /// <summary>
        /// Is this is a virtual fragment?
        /// </summary>
        public bool VirtualFragment => LcnOffset == 0;

        public readonly ulong StartVcn;

        public ulong LastVcn => StartVcn + RunLength - 1;

        private DataBlock(ushort lengthFieldLength, ushort offsetFieldLength, uint runLength, ulong lcnOffset, ulong startVcn)
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

            ulong runOffset = 0;

            if (offsetBytes > 0)
            {
                for (var i = 0; i < offsetBytes; i++)
                {
                    runOffset |= (ulong)data[offset + i] << (i * 8);
                }

                offset += offsetBytes;
            }

            return new DataBlock(lengthBytes, offsetBytes, runLength, runOffset, startVcn);
        }
    }
}
