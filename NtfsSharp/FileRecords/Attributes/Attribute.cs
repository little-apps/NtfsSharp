using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.FileRecords.Attributes
{
    public class Attribute
    {
        public AttributeHeaderBase Header { get; }
        public AttributeBodyBase Body { get; }

        public Attribute(AttributeHeaderBase header, AttributeBodyBase body)
        {
            Header = header;
            Body = body;
        }
    }
}
