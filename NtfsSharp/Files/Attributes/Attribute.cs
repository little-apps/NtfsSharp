using NtfsSharp.Files.Attributes.Base;

namespace NtfsSharp.Files.Attributes
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
