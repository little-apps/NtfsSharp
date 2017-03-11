using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.FileRecords.Attributes
{
    /// <summary>
    /// Similar to ::$DATA but operations are logged to the NTFS change journal.
    /// </summary>
    /// <remarks>
    /// Used by the Encrypting File System (EFS). All encrypted files have this attribute with the name $EFS.
    /// </remarks>
    public class LoggedUtilityStream : AttributeBodyBase
    {
        public LoggedUtilityStream(AttributeHeader header) : base(header)
        {
        }
    }
}
