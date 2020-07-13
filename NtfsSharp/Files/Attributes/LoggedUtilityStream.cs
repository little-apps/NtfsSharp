using NtfsSharp.Files.Attributes.Base;
using NtfsSharp.Files.Attributes.MetaData;

namespace NtfsSharp.Files.Attributes
{
    /// <summary>
    /// Similar to ::$DATA but operations are logged to the NTFS change journal.
    /// </summary>
    /// <remarks>
    /// Used by the Encrypting File System (EFS). All encrypted files have this attribute with the name $EFS.
    /// </remarks>
    [Resident(AttributeHeaderBase.NTFS_ATTR_TYPE.LOGGED_UTILITY_STREAM)]
    [NonResident(AttributeHeaderBase.NTFS_ATTR_TYPE.LOGGED_UTILITY_STREAM)]
    public sealed class LoggedUtilityStream : AttributeBodyBase
    {
        public LoggedUtilityStream(AttributeHeaderBase header) : base(header)
        {
        }

        public override string ToString()
        {
            return "$LOGGED_UTILITY_STREAM (0x100)";
        }
    }
}
