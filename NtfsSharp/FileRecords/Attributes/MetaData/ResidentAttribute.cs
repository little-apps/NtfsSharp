using System;
using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.FileRecords.Attributes.MetaData
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
