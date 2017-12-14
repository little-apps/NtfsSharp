using System;

namespace NtfsSharp.Tests.Driver.Attributes.NonResident
{
    public class DataRun
    {
        public const uint NibbleMax = byte.MaxValue >> 4;

        private byte _lengthSize = 0;
        private byte _lcnOffsetSize = 0;

        public ulong Length { get; set; }
        public ulong LcnOffset { get; set; }

        /// <summary>
        /// The number of bytes for the length. If zero, it is determined automatically. (default: 0)
        /// </summary>
        /// <remarks>This is actually a nibble (4 bits).</remarks>
        public byte LengthSize {
            get { return _lengthSize; }
            set
            {
                if (value > NibbleMax)
                    throw new ArgumentOutOfRangeException(nameof(value), $"Cannot be greater than {NibbleMax}");

                _lengthSize = value;
            }
        }

        /// <summary>
        /// The number of bytes for the LCN offset. If zero, it is determined automatically. (default: 0)
        /// </summary>
        /// <remarks>This is actually a nibble (4 bits).</remarks>
        public byte LcnOffsetSize
        {
            get { return _lcnOffsetSize; }
            set
            {
                if (value > NibbleMax)
                    throw new ArgumentOutOfRangeException(nameof(value), $"Cannot be greater than {NibbleMax}");

                _lcnOffsetSize = value;
            }
        }

        public DataRun(ulong length, ulong lcnOffset, byte lengthSize = 0, byte lcnOffsetSize = 0)
        {
            Length = length;
            LcnOffset = lcnOffset;
            LengthSize = lengthSize;
            LcnOffsetSize = lcnOffsetSize;
        }

        public byte[] GetDataRunBytes()
        {
            byte[] lengthBytes, lcnOffsetBytes;

            if (LengthSize > 0)
            {
                lengthBytes = new byte[LengthSize];
                Array.Copy(BitConverter.GetBytes(Length), 0, lengthBytes, 0, lengthBytes.Length);
            }
            else
            {
                lengthBytes = GetOnlyNeededBytes(Length);
            }

            if (LcnOffsetSize > 0)
            {

                lcnOffsetBytes = new byte[LcnOffsetSize];
                Array.Copy(BitConverter.GetBytes(LcnOffset), 0, lcnOffsetBytes, 0, lcnOffsetBytes.Length);
            }
            else
            {
                lcnOffsetBytes = GetOnlyNeededBytes(LcnOffset);
            }

            // Allocate bytes for data run
            var dataRunBytes = new byte[1 + lengthBytes.Length + lcnOffsetBytes.Length];

            // The first is two nibbles (one byte)
            dataRunBytes[0] = (byte) (((byte) lcnOffsetBytes.Length << 4) | (byte) lengthBytes.Length);

            Array.Copy(lengthBytes, 0, dataRunBytes, 1, lengthBytes.Length);
            Array.Copy(lcnOffsetBytes, 0, dataRunBytes, 1 + lengthBytes.Length, lcnOffsetBytes.Length);

            return dataRunBytes;
        }

        /// <summary>
        /// Gets only the used bytes from the number
        /// </summary>
        /// <param name="value">Number</param>
        /// <returns>Byte array with only used bytes</returns>
        private static byte[] GetOnlyNeededBytes(ulong value)
        {
            var valueBytes = BitConverter.GetBytes(value);
            var lastByteUsed = 0;
            
            if ((value & 0x8000000000000000) != 0x8000000000000000)
            {
                // If value is positive (or last bit is 0) -> loop through to find last byte used

                for (var i = valueBytes.Length - 1; i >= 0; i--)
                {
                    if (valueBytes[i] == 0)
                        continue;

                    lastByteUsed = i + 1;
                    break;
                }

                // Make sure last bit is not 1 (making the value negative when it's not)
                if (lastByteUsed < 8 && (valueBytes[lastByteUsed - 1] & 0x80) == 0x80)
                    lastByteUsed++;
            }
            else
            {
                // If value is negative (or last bit is 1) -> determine the least amount of bytes needed

                for (var i = valueBytes.Length - 1; i >= 0; i--)
                {
                    // If value bits are all 1 and i isn't 0 -> not the last byte used
                    if (valueBytes[i] == 0xff && i > 0)
                        continue;

                    // Set last byte used to i + 1
                    lastByteUsed = i + 1;

                    // Check if the last bit is 1 and if it isn't, the byte before is needed (in order to make it negative)
                    // ie: If the bit sequence is 1111 1111 0111 1111 and we use the second byte, it won't be negative.
                    if ((valueBytes[i] & 0x80) != 0x80)
                        lastByteUsed++;

                    break;
                }
            }
            

            var bytes = new byte[lastByteUsed];

            if (lastByteUsed > 0)
                Array.Copy(valueBytes, 0, bytes, 0, lastByteUsed);

            return bytes;
        }
    }
}
