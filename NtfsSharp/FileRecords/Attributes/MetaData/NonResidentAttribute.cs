using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.FileRecords.Attributes.MetaData
{
    /// <summary>
    /// Used to declare classes that are for non-resident attributes
    /// </summary>
    internal class NonResidentAttribute : MetaDataBase
    {
        internal NonResidentAttribute(AttributeHeaderBase.NTFS_ATTR_TYPE attrType) : base(attrType, true)
        {

        }
    }
}
