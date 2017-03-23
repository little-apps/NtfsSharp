using System;
using System.Collections.Generic;
using NtfsSharp.FileRecords.Attributes.IndexRoot;
using NtfsSharp.Helpers;
using static NtfsSharp.FileRecords.Attributes.Base.AttributeHeaderBase;

namespace NtfsSharp.FileRecords.Attributes.Base
{
    public sealed class AttributeBase
    {
        public AttributeHeaderBase Header { get; }
        public AttributeBodyBase Body { get; private set; }
        
        private static readonly Dictionary<NTFS_ATTR_TYPE, Type> Attributes = new Dictionary<NTFS_ATTR_TYPE, Type>
        {
            {NTFS_ATTR_TYPE.STANDARD_INFORMATION, typeof(StandardInformation)},
            {NTFS_ATTR_TYPE.FILE_NAME, typeof(FileNameAttribute)},
            {NTFS_ATTR_TYPE.VOLUME_NAME, typeof(VolumeName)},
            {NTFS_ATTR_TYPE.VOLUME_INFORMATION, typeof(VolumeInformation)},
            {NTFS_ATTR_TYPE.INDEX_ROOT, typeof(Root)},
            {NTFS_ATTR_TYPE.LOGGED_UTILITY_STREAM, typeof(LoggedUtilityStream)},
            {NTFS_ATTR_TYPE.DATA, typeof(DataAttribute)},
            {NTFS_ATTR_TYPE.OBJECT_ID, typeof(ObjectId)},
            {NTFS_ATTR_TYPE.SECURITY_DESCRIPTOR, typeof(SecurityDescriptor)},
            {NTFS_ATTR_TYPE.BITMAP, typeof(BitmapAttribute)},
            {NTFS_ATTR_TYPE.INDEX_ALLOCATION, typeof(IndexAllocation.IndexAllocation)},
            {NTFS_ATTR_TYPE.ATTRIBUTE_LIST, typeof(AttributeList.AttributeList)},
            {NTFS_ATTR_TYPE.REPARSE_POINT, typeof(ReparsePoint)},
            {NTFS_ATTR_TYPE.EA_INFORMATION,  typeof(ExtendedAttributeInformation)},
            {NTFS_ATTR_TYPE.EA, typeof(ExtendedAttribute)}
        };

        /// <summary>
        /// Creates attribute from bytes
        /// </summary>
        /// <param name="bytes">Bytes of data</param>
        /// <param name="fileRecord">File record holding attribute</param>
        public AttributeBase(byte[] bytes, FileRecord fileRecord)
        {
            var header = bytes.ToStructure<NTFS_ATTRIBUTE_HEADER>();

            AttributeHeaderBase attrHeader;

            if (header.NonResident)
                attrHeader = new NonResident.NonResident(header, bytes, fileRecord);
            else
                attrHeader = new Resident(header, bytes, fileRecord);

            Header = attrHeader;
            ReadBody();
        }

        public AttributeBase ReadBody()
        {
            var type = GetClassTypeFromType(Header.Header.Type);

            Body = (AttributeBodyBase)Activator.CreateInstance(type, Header);

            return this;
        }

        public static Type GetClassTypeFromType(NTFS_ATTR_TYPE type)
        {
            if (!Attributes.ContainsKey(type))
                throw new Exceptions.InvalidAttributeException("Attribute type is invalid");

            return Attributes[type];
        }
    }
}
