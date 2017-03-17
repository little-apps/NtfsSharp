using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.FileRecords.Attributes
{
    public class DataAttribute : AttributeBodyBase
    {
        public byte[] Data => Body;

        public DataAttribute(AttributeHeader header) : base(header, MustBe.Resident | MustBe.NonResident, false)
        {
        }
    }
}
