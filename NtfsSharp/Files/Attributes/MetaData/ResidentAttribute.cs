using NtfsSharp.Files.Attributes.Base;

namespace NtfsSharp.Files.Attributes.MetaData
{
    /// <summary>
    /// Used to declare classes that are for resident attributes
    /// </summary>
    internal class ResidentAttribute : MetaDataBase
    {
        internal ResidentAttribute(AttributeHeaderBase.NTFS_ATTR_TYPE attrType) : base(attrType, false)
        {
        }
    }
}
