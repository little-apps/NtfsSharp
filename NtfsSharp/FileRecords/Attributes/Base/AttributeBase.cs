using System;
using System.Collections.Generic;
using System.Reflection;
using NtfsSharp.Exceptions;
using NtfsSharp.FileRecords.Attributes.MetaData;
using NtfsSharp.Helpers;
using static NtfsSharp.FileRecords.Attributes.Base.AttributeHeaderBase;

namespace NtfsSharp.FileRecords.Attributes.Base
{
    public sealed class AttributeBase
    {
        public AttributeHeaderBase Header { get; }
        public AttributeBodyBase Body { get; private set; }

        private static bool _builtAttributeTypes;
        private static Dictionary<NTFS_ATTR_TYPE, Type> _residentTypes = new Dictionary<NTFS_ATTR_TYPE, Type>();
        private static Dictionary<NTFS_ATTR_TYPE, Type> _nonResidentTypes = new Dictionary<NTFS_ATTR_TYPE, Type>();

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
            if (!_builtAttributeTypes)
            {
                BuildAttributes();
                _builtAttributeTypes = true;
            }
            
            var type = !Header.Header.NonResident
                ? GetResidentClassFromType(Header.Header.Type)
                : GetNonResidentClassFromType(Header.Header.Type);

            Body = (AttributeBodyBase)Activator.CreateInstance(type, Header);

            return this;
        }

        private static Type GetResidentClassFromType(NTFS_ATTR_TYPE ntfsAttrType)
        {
            if (!_residentTypes.ContainsKey(ntfsAttrType))
                throw new InvalidAttributeException(_nonResidentTypes.ContainsKey(ntfsAttrType)
                    ? "Attribute can only resident."
                    : "Attribute type is invalid.");

            return _residentTypes[ntfsAttrType];
        }

        private static Type GetNonResidentClassFromType(NTFS_ATTR_TYPE ntfsAttrType)
        {
            if (!_nonResidentTypes.ContainsKey(ntfsAttrType))
                throw new InvalidAttributeException(_residentTypes.ContainsKey(ntfsAttrType)
                    ? "Attribute can only resident."
                    : "Attribute type is invalid.");

            return _nonResidentTypes[ntfsAttrType];
        }

        private void BuildAttributes()
        {
            _residentTypes = new Dictionary<NTFS_ATTR_TYPE, Type>();
            _nonResidentTypes = new Dictionary<NTFS_ATTR_TYPE, Type>();

            foreach (var type in Assembly.GetAssembly(GetType()).GetTypes())
            {
                var residentAttribute = (ResidentAttribute) type.GetCustomAttribute(typeof(ResidentAttribute));
                var nonResidentAttribute = (NonResidentAttribute) type.GetCustomAttribute(typeof(NonResidentAttribute));

                if (residentAttribute != null)
                    _residentTypes.Add(residentAttribute.AttributeType, type);

                if (nonResidentAttribute != null)
                    _nonResidentTypes.Add(nonResidentAttribute.AttributeType, type);
            }
        }
    }
}
