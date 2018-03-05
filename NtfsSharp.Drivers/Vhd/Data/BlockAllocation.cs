using System;
using System.Collections;
using System.Linq;

namespace VhdParser.Data
{
    public class BlockAllocation
    {
        public BitArray Bitmap { get; }
        public uint[] Entries { get; }

        public uint this[uint i] => Entries[i];

        public BlockAllocation(byte[] bytes)
        {
            Bitmap = new BitArray(bytes.Length / 4);
            Entries = new uint[bytes.Length / 4];

            for (var i = 0; i < bytes.Length; i += 4)
            {
                // Get bytes in big endian/reverse order
                var entryBytes = new byte[4];

                Array.Copy(bytes, i, entryBytes, 0, 4);
                
                Entries[i / 4] = BitConverter.ToUInt32(entryBytes.Reverse().ToArray(), 0);
                Bitmap.Set(i / 4, Entries[i / 4] != uint.MaxValue);
            }
        }


    }
}
