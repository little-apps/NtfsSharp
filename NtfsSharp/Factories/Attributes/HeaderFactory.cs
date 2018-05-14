using NtfsSharp.FileRecords;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.Base.NonResident;
using NtfsSharp.Helpers;
using static NtfsSharp.FileRecords.Attributes.Base.AttributeHeaderBase;

namespace NtfsSharp.Factories.Attributes
{
    public class HeaderFactory
    {
        /// <summary>
        /// Builds the header of an attribute
        /// </summary>
        /// <param name="headerBytes">Bytes containing header</param>
        /// <param name="fileRecord">File record where attribute exists</param>
        /// <returns>Attribute header</returns>
        public AttributeHeaderBase Build(byte[] headerBytes, FileRecord fileRecord)
        {
            var header = headerBytes.ToStructure<NTFS_ATTRIBUTE_HEADER>();

            if (header.NonResident)
                return new NonResident(header, headerBytes, fileRecord);
            else
                return new Resident(header, headerBytes, fileRecord);
        }
    }
}
