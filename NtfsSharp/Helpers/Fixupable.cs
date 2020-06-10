using System;
using NtfsSharp.Exceptions;

namespace NtfsSharp.Helpers
{
    public class Fixupable
    {
        /// <summary>
        /// Bytes expected at the end of each sector for file record.
        /// </summary>
        public byte[] EndTag { get; }

        /// <summary>
        /// Bytes to be replaced at end of each sector for file record.
        /// </summary>
        public byte[] UpdateSequenceArray { get; }

        /// <summary>
        /// Number of bytes in a sector
        /// </summary>
        public ushort BytesPerSector { get; }

        public Fixupable(byte[] endTag, byte[] updateSequenceArray, ushort bytesPerSector)
        {
            if (endTag == null)
                throw new ArgumentNullException(nameof(endTag));

            if (endTag.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(endTag), "End tag bytes cannot be empty.");

            if (updateSequenceArray == null)
                throw new ArgumentNullException(nameof(updateSequenceArray));

            if (updateSequenceArray.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(updateSequenceArray), "Update sequence array cannot be empty.");

            EndTag = endTag;
            UpdateSequenceArray = updateSequenceArray;
            BytesPerSector = bytesPerSector;
        }

        /// <summary>
        /// Checks if last two bytes of each sector in file record match first two bytes of update sequence array.
        /// If they do, replace the last two bytes of each sector with the corresponding bytes in the fixup array.
        /// </summary>
        /// <param name="data">Bytes containing sectors to fix up</param>
        /// <exception cref="InvalidEndTagsException">Thrown if end tags do not match</exception>
        public void Fixup(byte[] data)
        {
            // Fixup sectors
            for (var i = 0; i < UpdateSequenceArray.Length; i++)
            {
                // What sector index is it?
                // Take the index in the USA, divide it by 2.
                var sectorIndex = i / 2;

                // Multiply the sectorIndex by the bytes per sector to get the first index for the sector. Add bytes per sector - 1 to get the last index for the sector.
                var sectorEndIndex = sectorIndex * BytesPerSector + (BytesPerSector - 1);

                // Which byte in end tag is it?
                var endTagOffset = i % 2;

                // The index of the byte for the current USA item is the sectorEndIndex subtract 1 if it's the first or 0 if it's the last.
                var sectorUsaOffset = sectorEndIndex - (1 - endTagOffset);

                // Do last two bytes in sector match end tag?
                if (data[sectorUsaOffset] != EndTag[endTagOffset])
                    throw new InvalidEndTagsException(i);

                // Replace corresponding bytes in sector with corresponding bytes in USA
                data[sectorUsaOffset] = UpdateSequenceArray[i];
            }
        }
    }
}
