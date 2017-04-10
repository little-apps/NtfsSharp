using System;

namespace NtfsSharp.Helpers
{
    public abstract class Fixupable
    {
        public byte[] EndTag { get; private set; }
        public byte[] UpdateSequenceArray { get; private set; }

        /// <summary>
        /// Checks if last two bytes of each sector in file record match first two bytes of update sequence array.
        /// If they do, replace the last two bytes of each sector with the corresponding bytes in the fixup array.
        /// </summary>
        /// <param name="data">Bytes containing sectors to fix up</param>
        /// <param name="usaOffset">Offset of update sequeunce array in <see cref="data"/></param>
        /// <param name="usaSize">Size of update sequeunce array in <see cref="data"/></param>
        /// <param name="bytesPerSector">Bytes per sector (usually 512)</param>
        /// <param name="invalidSector">If this fails, this is the sector # in <see cref="data"/> that's invalid</param>
        /// <returns>True if end tags match and fixup was performed</returns>
        protected bool Fixup(byte[] data, ushort usaOffset, ushort usaSize, ushort bytesPerSector, out short invalidSector)
        {
            // Get end tag + USA
            EndTag = new byte[2];
            UpdateSequenceArray = new byte[(usaSize - 1) * 2];

            // First two bytes of USA are end tag
            Array.Copy(data, usaOffset, EndTag, 0, 2);
            // Rest of bytes in USA are fixup bytes
            Array.Copy(data, usaOffset + 2, UpdateSequenceArray, 0, UpdateSequenceArray.Length);

            // Fixup sectors
            for (var i = 1; i < usaSize; i++)
            {
                // Offset of last two bytes in sector
                var sectorUsaOffset = i * bytesPerSector - 2;

                // Do last two bytes in sector match end tag?
                if (data[sectorUsaOffset] != EndTag[0] || data[sectorUsaOffset + 1] != EndTag[1])
                {
                    invalidSector = (short) (i - 1);
                    return false;
                }
                    

                // Replace last two bytes in sector with corresponding bytes in USA
                data[sectorUsaOffset] = UpdateSequenceArray[(i - 1) * 2];
                data[sectorUsaOffset] = UpdateSequenceArray[(i - 1) * 2 + 1];
            }

            invalidSector = -1;
            return true;
        }
    }
}
