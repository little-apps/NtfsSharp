using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.FileRecords.Attributes
{
    public class DataAttribute : AttributeBodyBase
    {
        public byte[] Data => Header.BodyData;

        public DataAttribute(AttributeHeader header) : base(header)
        {
        }
    }
}
