using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.MetaData;

namespace NtfsSharp.FileRecords.Attributes
{
    /// <summary>
    /// This attribute contains the file's data. A file's size is the size of its unnamed Data Stream.
    /// </summary>
    [Resident(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA)]
    [NonResident(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA)]
    public sealed class DataAttribute : AttributeBodyBase
    {
        /// <summary>
        /// Represents file contents
        /// </summary>
        /// <remarks>
        /// The ReadBody method in Header must be called, otherwise, this property is null. Even then, it is dangerous to use this as the contents will be stored in memory (potentially causing a memory overflow).
        /// TODO: Create Stream to read contents directly from disk
        /// </remarks>
        public byte[] Data => Body;

        public DataAttribute(AttributeHeaderBase header) : base(header, false)
        {
        }

        public override string ToString()
        {
            return "$DATA (0x80)";
        }
    }
}
