using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.Shared;

namespace NtfsSharp.FileRecords.Attributes
{
    public class FileNameAttribute : AttributeBodyBase
    {
        public readonly FileName FileName;

        public FileNameAttribute(AttributeHeader header) : base(header)
        {
            FileName = new FileName(Bytes, CurrentOffset);
            CurrentOffset = FileName.CurrentOffset;
        }
        
    }
}
