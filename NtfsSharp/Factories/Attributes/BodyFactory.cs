using NtfsSharp.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;
using NtfsSharp.Files.Attributes.Base;
using NtfsSharp.Files.Attributes.MetaData;
using static NtfsSharp.Files.Attributes.Base.AttributeHeaderBase;

namespace NtfsSharp.Factories.Attributes
{
    public class BodyFactory
    {
        private static bool _builtAttributeTypes;
        private static Dictionary<NTFS_ATTR_TYPE, Type> _residentTypes = new Dictionary<NTFS_ATTR_TYPE, Type>();
        private static Dictionary<NTFS_ATTR_TYPE, Type> _nonResidentTypes = new Dictionary<NTFS_ATTR_TYPE, Type>();

        /// <summary>
        /// Builds the body of an attribute
        /// </summary>
        /// <param name="header">Header of attribute</param>
        /// <returns>Attribute body</returns>
        public AttributeBodyBase Build(AttributeHeaderBase header)
        {
            if (!_builtAttributeTypes)
            {
                BuildAttributes();
                _builtAttributeTypes = true;
            }

            var type = !header.Header.NonResident
                ? GetResidentClassFromType(header.Header.Type)
                : GetNonResidentClassFromType(header.Header.Type);

            return (AttributeBodyBase) Activator.CreateInstance(type, header);
        }

        private Type GetResidentClassFromType(NTFS_ATTR_TYPE ntfsAttrType)
        {
            if (!_residentTypes.ContainsKey(ntfsAttrType))
                throw new InvalidAttributeException(_nonResidentTypes.ContainsKey(ntfsAttrType)
                    ? "Attribute can only be non-resident."
                    : "Attribute type is invalid.");

            return _residentTypes[ntfsAttrType];
        }

        private Type GetNonResidentClassFromType(NTFS_ATTR_TYPE ntfsAttrType)
        {
            if (!_nonResidentTypes.ContainsKey(ntfsAttrType))
                throw new InvalidAttributeException(_residentTypes.ContainsKey(ntfsAttrType)
                    ? "Attribute can only be resident."
                    : "Attribute type is invalid.");

            return _nonResidentTypes[ntfsAttrType];
        }

        private void BuildAttributes()
        {
            _residentTypes = new Dictionary<NTFS_ATTR_TYPE, Type>();
            _nonResidentTypes = new Dictionary<NTFS_ATTR_TYPE, Type>();

            foreach (var type in Assembly.GetAssembly(GetType()).GetTypes())
            {
                var residentAttribute = (ResidentAttribute)type.GetCustomAttribute(typeof(ResidentAttribute));
                var nonResidentAttribute = (NonResidentAttribute)type.GetCustomAttribute(typeof(NonResidentAttribute));

                if (residentAttribute != null)
                    _residentTypes.Add(residentAttribute.AttributeType, type);

                if (nonResidentAttribute != null)
                    _nonResidentTypes.Add(nonResidentAttribute.AttributeType, type);
            }
        }
    }
}
