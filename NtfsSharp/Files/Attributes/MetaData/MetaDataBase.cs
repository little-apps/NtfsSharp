using System;
using NtfsSharp.Files.Attributes.Base;

namespace NtfsSharp.Files.Attributes.MetaData
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
