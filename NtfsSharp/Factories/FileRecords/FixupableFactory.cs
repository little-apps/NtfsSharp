using NtfsSharp.Helpers;
using System;

namespace NtfsSharp.Factories.FileRecords
{
    public static class FixupableFactory
    {
        /// <summary>
        /// Builds <seealso cref="Fixupable"/> object
        /// </summary>
        /// <param name="fileRecordBytes">Bytes containing file record</param>
        /// <param name="bytesPerSector">Number of bytes in a sector</param>
        /// <returns><seealso cref="Fixupable"/> object to perform fixup</returns>
        public static Fixupable Build(byte[] fileRecordBytes, ushort bytesPerSector)
        {
            // USA offset is at offset 4 and is 2 bytes
            var usaOffset = BitConverter.ToUInt16(fileRecordBytes, 4);

            // USA size is at offset 6 and is 2 bytes
            var usaSize = BitConverter.ToUInt16(fileRecordBytes, 6);

            // Get end tag + USA
            var endTag = new byte[2];
            var updateSequenceArray = new byte[(usaSize - 1) * 2];

            // First two bytes of USA are end tag
            Array.Copy(fileRecordBytes, usaOffset, endTag, 0, 2);
            // Rest of bytes in USA are fixup bytes
            Array.Copy(fileRecordBytes, usaOffset + 2, updateSequenceArray, 0, updateSequenceArray.Length);

            return new Fixupable(endTag, updateSequenceArray, bytesPerSector);
        }
    }
}
