using System;
using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.FileRecords.Attributes.MetaData
{
    [AttributeUsage(AttributeTargets.Class)]
    internal abstract class MetaDataBase : System.Attribute
    {
        internal readonly AttributeHeaderBase.NTFS_ATTR_TYPE AttributeType;
        internal readonly bool NonResident;

        internal MetaDataBase(AttributeHeaderBase.NTFS_ATTR_TYPE attrType, bool nonResident)
        {
            AttributeType = attrType;
            NonResident = nonResident;
        }
    }
}
