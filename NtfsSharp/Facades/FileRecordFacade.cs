using System;
using System.Linq;
using NtfsSharp.Exceptions;
using NtfsSharp.Factories.FileRecords;
using NtfsSharp.FileRecords;
using NtfsSharp.Helpers;
using NtfsSharp.Volumes;

namespace NtfsSharp.Facades
{
    public static class FileRecordFacade
    {
        /// <summary>
        /// Builds a FileRecord using data and fixes it up (from the update sequence array)
        /// </summary>
        /// <param name="data">Bytes containing file record</param>
        /// <param name="reader">Reader that read the file record bytes</param>
        /// <returns><seealso cref="FileRecord"/> object</returns>
        /// <exception cref="InvalidFileRecordException">Thrown when last 2 bytes do not match update sequence array end tag.</exception>
        public static FileRecord Build(byte[] data, IVolume reader)
        {
            var header = data.ToStructure<FileRecord.FILE_RECORD_HEADER_NTFS>();

            // Is it a FILE record?
            if (!header.Magic.SequenceEqual(new byte[] { 0x46, 0x49, 0x4C, 0x45 }))
                throw new InvalidFileRecordException(nameof(header.Magic), null);

            if (header.UpdateSequenceSize - 1 > reader.SectorsPerMftRecord)
                throw new InvalidFileRecordException(nameof(header.UpdateSequenceSize), "Update sequence size exceeds number of sectors in file record", null);

            var fixupable = FixupableFactory.Build(data, reader.BytesPerSector);

            try
            {
                fixupable.Fixup(data);
            }
            catch (InvalidEndTagsException ex)
            {
                throw new InvalidFileRecordException(nameof(fixupable.EndTag),
                    $"Last 2 bytes of sector {ex.InvalidSector} don't match update sequence array end tag.", null);
            }

            var fileRecord = BytesFactory.Build(data, reader);
            fileRecord.Fixupable = fixupable;
            
            return fileRecord;
        }
    }
}
