using NtfsSharp.FileRecords.Attributes.Base;
using System.Text;
using NtfsSharp.FileRecords.Attributes.MetaData;

namespace NtfsSharp.FileRecords.Attributes
{
    /// <summary>
    /// This attribute simply contains the name of the volume.
    /// </summary>
    [Resident(AttributeHeaderBase.NTFS_ATTR_TYPE.VOLUME_NAME)]
    public sealed class VolumeName : AttributeBodyBase
    {
        public readonly string Name;

        public VolumeName(AttributeHeaderBase header) : base(header)
        {
            var residentHeader = header as Resident;

            Name = Encoding.Unicode.GetString(GetBytesFromCurrentOffset(residentHeader.SubHeader.AttributeLength));
            CurrentOffset += residentHeader.SubHeader.AttributeLength;
        }

        public override string ToString()
        {
            return "$VOLUME_NAME (0x60)";
        }
    }
}
