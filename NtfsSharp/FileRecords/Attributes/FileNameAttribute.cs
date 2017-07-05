using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.MetaData;
using NtfsSharp.FileRecords.Attributes.Shared;

namespace NtfsSharp.FileRecords.Attributes
{
    /// <summary>
    /// This Attribute stores the name of the file attribute and is always resident. 
    /// </summary>
    [Resident(AttributeHeaderBase.NTFS_ATTR_TYPE.FILE_NAME)]
    public class FileNameAttribute : AttributeBodyBase
    {
        public readonly FileName FileName;

        public FileNameAttribute(AttributeHeaderBase header) : base(header)
        {
            FileName = new FileName(Body, CurrentOffset);
            CurrentOffset = FileName.CurrentOffset;
        }

        public override string ToString()
        {
            return "$FILE_NAME (0x30)";
        }
    }
}
