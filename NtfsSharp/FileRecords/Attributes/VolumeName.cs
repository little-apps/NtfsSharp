using NtfsSharp.FileRecords.Attributes.Base;
using System.Text;

namespace NtfsSharp.FileRecords.Attributes
{
    public class VolumeName : AttributeBodyBase
    {
        public readonly string Name;

        public VolumeName(AttributeHeader header) : base(header, MustBe.Resident)
        {
            var residentHeader = header as Resident;

            Name = Encoding.Unicode.GetString(GetBytesFromCurrentOffset(residentHeader.SubHeader.AttributeLength));
            CurrentOffset += residentHeader.SubHeader.AttributeLength;
        }
    }
}
