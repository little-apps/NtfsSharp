using NtfsSharp.FileRecords.Attributes.Base;
using System.Collections;
using NtfsSharp.FileRecords.Attributes.MetaData;

namespace NtfsSharp.FileRecords.Attributes
{
    /// <summary>
    /// This file attribute is a sequence of bits, each of which represents the status of an entity. 
    /// </summary>
    [Resident(AttributeHeaderBase.NTFS_ATTR_TYPE.BITMAP)]
    [NonResident(AttributeHeaderBase.NTFS_ATTR_TYPE.BITMAP)]
    public class BitmapAttribute : AttributeBodyBase
    {
        /// <summary>
        /// In an index, the bit field shows which index entries are in use. Each bit represents one VCN of the index allocation. 
        /// In the $MFT, the bit field shows which FILE records are in use. 
        /// </summary>
        public readonly BitArray Bitmap;

        public BitmapAttribute(AttributeHeaderBase header) : base(header)
        {
            Bitmap = new BitArray(Body);
        }

        public override string ToString()
        {
            return "$BITMAP (0xB0)";
        }
    }
}
