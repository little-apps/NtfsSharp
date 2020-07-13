using NtfsSharp.Files.Attributes.Base;

namespace NtfsSharp.Files.Attributes.MetaData
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
