using NtfsSharp.FileRecords.Attributes.IndexRoot;
using NtfsSharp.Helpers;
using static NtfsSharp.FileRecords.Attributes.Base.AttributeHeader;

namespace NtfsSharp.FileRecords.Attributes.Base
{
    public abstract class AttributeBase
    {
        public readonly byte[] Bytes;
        public uint CurrentOffset { get; protected set; }

        protected AttributeBase(byte[] data)
        {
            Bytes = data;
        }

        protected byte[] GetBytesFromCurrentOffset(uint length)
        {
            return Bytes.GetBytesAtOffset(CurrentOffset, length);
        }

        /// <summary>
        /// Creates attribute from bytes
        /// </summary>
        /// <param name="bytes">Bytes of data</param>
        /// <returns>AttributesBase containing resident or non-resident data</returns>
        public static AttributeBodyBase GetAttribute(byte[] bytes)
        {
            var header = bytes.ToStructure<NTFS_ATTRIBUTE_HEADER>();

            AttributeHeader attrHeader;

            if (header.NonResident)
            {
                attrHeader = new NonResident.NonResident(header, bytes);
            }
            else
            {
                attrHeader = new Resident(header, bytes);
            }
            
            return ReadBody(attrHeader);
        }

        public static AttributeBodyBase ReadBody(AttributeHeader header)
        {
            switch (header.Header.Type)
            {
                case NTFS_ATTR_TYPE.STANDARD_INFORMATION:
                    return new StandardInformation(header);

                case NTFS_ATTR_TYPE.FILE_NAME:
                    return new FileNameAttribute(header);

                case NTFS_ATTR_TYPE.VOLUME_NAME:
                    return new VolumeName(header);

                case NTFS_ATTR_TYPE.VOLUME_INFORMATION:
                    return new VolumeInformation(header);

                case NTFS_ATTR_TYPE.INDEX_ROOT:
                    return new Root(header);

                case NTFS_ATTR_TYPE.DATA:

                case NTFS_ATTR_TYPE.OBJECT_ID:
                    return new ObjectId(header);

                case NTFS_ATTR_TYPE.SECURITY_DESCRIPTOR:
                    return new SecurityDescriptor(header);

                default:
                    throw new Exceptions.InvalidAttributeException("Attribute type is invalid");
            }
        }
    }
}
