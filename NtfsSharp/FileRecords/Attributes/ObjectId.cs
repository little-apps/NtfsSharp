using NtfsSharp.FileRecords.Attributes.Base;
using System;
using NtfsSharp.FileRecords.Attributes.MetaData;

namespace NtfsSharp.FileRecords.Attributes
{
    /// <summary>
    /// The Object Id was introduced in Windows 2000. Every MFT Record is assigned a unique GUID. Additionally, a record may have a Birth Volume Id, a Birth Object Id and a Domain Id, all of which are GUIDs. 
    /// </summary>
    [Resident(AttributeHeaderBase.NTFS_ATTR_TYPE.OBJECT_ID)]
    [NonResident(AttributeHeaderBase.NTFS_ATTR_TYPE.OBJECT_ID)]
    public class ObjectId : AttributeBodyBase
    {
        private const uint GuidSizeBytes = 16;

        public readonly Guid UniqueObjectId;
        public readonly Guid BirthVolumeId;
        public readonly Guid BirthObjectId;
        public readonly Guid DomainId;
        
        public ObjectId(AttributeHeaderBase header) : base(header)
        {
            if (OffsetWithHeader == header.Header.Length)
                return;

            UniqueObjectId = ReadNextGuid();

            if (OffsetWithHeader == header.Header.Length)
                return;

            BirthVolumeId = ReadNextGuid();

            if (OffsetWithHeader == header.Header.Length)
                return;

            BirthObjectId = ReadNextGuid();

            if (OffsetWithHeader == header.Header.Length)
                return;

            DomainId = ReadNextGuid();
        }

        private Guid ReadNextGuid()
        {
            var guid = new Guid(GetBytesFromCurrentOffset(GuidSizeBytes));
            CurrentOffset += GuidSizeBytes;

            return guid;
        }

        public override string ToString()
        {
            return "$OBJECT_ID (0x40)";
        }
    }
}
