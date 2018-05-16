using System;
using NtfsSharp.Exceptions;
using NtfsSharp.Factories.FileRecords;
using NtfsSharp.FileRecords;
using NtfsSharp.Helpers;
using NtfsSharp.Volumes;

namespace NtfsSharp.Facades
{
    public static class FileRecordFacade
    {
        private static readonly Fixupable Fixupable = new Fixupable();

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

            if (header.UpdateSequenceSize - 1 > reader.SectorsPerMftRecord)
                throw new InvalidFileRecordException(nameof(header.UpdateSequenceSize), "Update sequence size exceeds number of sectors in file record", null);

            try
            {
                Fixupable.Fixup(data, header.UpdateSequenceOffset, header.UpdateSequenceSize, reader.BytesPerSector);
            }
            catch (InvalidEndTagsException ex)
            {
                throw new InvalidFileRecordException(nameof(Fixupable.EndTag),
                    $"Last 2 bytes of sector {ex.InvalidSector} don't match update sequence array end tag.", null);
            }

            var fileRecord = BytesFactory.Build(data, reader);
            fileRecord.Fixupable = Fixupable;
            
            return fileRecord;
        }
    }
}
