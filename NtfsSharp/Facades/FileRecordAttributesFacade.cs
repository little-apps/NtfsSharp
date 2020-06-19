using System;
using NtfsSharp.Factories.Attributes;
using NtfsSharp.FileRecords;
using NtfsSharp.Volumes;

namespace NtfsSharp.Facades
{
    public static class FileRecordAttributesFacade
    {
        /// <summary>
        /// Creates a <seealso cref="FileRecord"/> object, fixes it up and associates it's attributes
        /// </summary>
        /// <param name="data">Data containing file record header and attributes</param>
        /// <param name="reader">Where the data was read from</param>
        /// <returns><seealso cref="FileRecord"/> object with attributes.</returns>
        public static FileRecord Build(byte[] data, Volume reader)
        {
            var fileRecord = FileRecordFacade.Build(data, reader);

            uint currentOffset = fileRecord.Header.FirstAttributeOffset;

            while (currentOffset < data.Length && BitConverter.ToUInt32(data, (int) currentOffset) != 0xffffffff)
            {
                var newData = new byte[data.Length - currentOffset];
                Array.Copy(data, currentOffset, newData, 0, newData.Length);

                var attribute = AttributeFactory.Build(newData, fileRecord);
                
                fileRecord.Attributes.Add(attribute);

                currentOffset += attribute.Header.Header.Length;
            }

            return fileRecord;
        }
    }
}
