using System.Collections.Generic;
using NtfsSharp.Exceptions;
using NtfsSharp.Files.Attributes.Base;
using NtfsSharp.Files.Attributes.Base.NonResident;
using NtfsSharp.Files.Attributes.MetaData;

namespace NtfsSharp.Files.Attributes.AttributeList
{
    /// <summary>
    /// When there are lots of attributes and space in the MFT record is short, all those attributes that can be made non-resident are moved out of the MFT. If there is still not enough room, then an $ATTRIBUTE_LIST attribute is needed. The remaining attributes are placed in a new MFT record and the $ATTRIBUTE_LIST describes where to find them. It is very unusual to see this attribute.
    /// </summary>
    [Resident(AttributeHeaderBase.NTFS_ATTR_TYPE.ATTRIBUTE_LIST)]
    [NonResident(AttributeHeaderBase.NTFS_ATTR_TYPE.ATTRIBUTE_LIST)]
    public class AttributeList : AttributeBodyBase
    {
        public readonly List<AttributeListItem> AttributeListItems = new List<AttributeListItem>();

        /// <summary>
        /// Represents $ATTRIBUTE_LIST
        /// </summary>
        /// <param name="header"></param>
        public AttributeList(AttributeHeaderBase header) : base(header)
        {
            var bodyLength = GetBodyLength();

            while (CurrentOffset < bodyLength)
            {
                var attrItem = new AttributeListItem(this);

                AttributeListItems.Add(attrItem);
                CurrentOffset += attrItem.Header.Length;
            }
        }

        /// <summary>
        /// Gets the length of the body of the attribute list
        /// </summary>
        /// <returns>Length (in bytes)</returns>
        /// <exception cref="InvalidAttributeException">Thrown if header is neither resident nor non-resident</exception>
        private ulong GetBodyLength()
        {
            switch (Header)
            {
                case Resident resident:
                    return resident.SubHeader.AttributeLength;
                case NonResident nonResident:
                    return nonResident.SubHeader.AttributeSize;
            }

            throw new InvalidAttributeException("Unable to determine if resident or non-resident attribute.");
        }
    }
}
