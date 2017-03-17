using System.Collections.Generic;
using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.FileRecords.Attributes.AttributeList
{
    /// <summary>
    /// When there are lots of attributes and space in the MFT record is short, all those attributes that can be made non-resident are moved out of the MFT. If there is still not enough room, then an $ATTRIBUTE_LIST attribute is needed. The remaining attributes are placed in a new MFT record and the $ATTRIBUTE_LIST describes where to find them. It is very unusual to see this attribute. 
    /// </summary>
    public class AttributeList : AttributeBodyBase
    {
        public readonly List<AttributeListItem> AttributeListItems = new List<AttributeListItem>();

        /// <summary>
        /// Represents $ATTRIBUTE_LIST
        /// </summary>
        /// <param name="header"></param>
        public AttributeList(AttributeHeader header) : base(header)
        {
            while (CurrentOffset <= header.Header.Length)
            {
                var attrItem = new AttributeListItem(this);

                AttributeListItems.Add(attrItem);
                CurrentOffset += attrItem.Header.Length;
            }
        }
        
        
    }
}
