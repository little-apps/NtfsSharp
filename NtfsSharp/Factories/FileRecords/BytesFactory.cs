using System;
using System.Linq;
using NtfsSharp.Exceptions;
using NtfsSharp.FileRecords;
using NtfsSharp.Helpers;
using NtfsSharp.Volumes;

namespace NtfsSharp.Factories.FileRecords
{
    public static class BytesFactory
    {
        /// <summary>
        /// Creates a <seealso cref="FileRecord"/> object from bytes
        /// </summary>
        /// <param name="data">Bytes containing the file record</param>
        /// <param name="reader">Where the data was read from</param>
        /// <returns><seealso cref="FileRecord"/> object</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="data"/> is empty</exception>
        /// <exception cref="InvalidFileRecordException">Thrown if unable to read file record</exception>
        public static FileRecord Build(byte[] data, IVolume reader)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null");

            if (data.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(data), "Data cannot be empty");

            var header = data.ToStructure<FileRecord.FILE_RECORD_HEADER_NTFS>();

            if (!header.Magic.SequenceEqual(new byte[] { 0x46, 0x49, 0x4C, 0x45 }))
                throw new InvalidFileRecordException(nameof(header.Magic), null);

            return new FileRecord(header, reader);
        }
    }
}
