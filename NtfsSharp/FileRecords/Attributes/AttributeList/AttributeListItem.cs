using System;
using System.Linq;
using System.Text;
using NtfsSharp.Exceptions;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.Base.NonResident;
using NtfsSharp.Helpers;
using NtfsSharp.PInvoke;

namespace NtfsSharp.FileRecords.Attributes.AttributeList
{
    public class AttributeListItem
    {
        public readonly NTFS_ATTRIBUTE_LIST_HEADER Header;

        /// <summary>
        /// Name of attribute
        /// </summary>
        /// <remarks>Null if NameLength and/or NameOffset is 0</remarks>
        public readonly string Name;

        /// <summary>
        /// Attribute located in child file record
        /// </summary>
        /// <remarks>Can be null if there is attribute is located in this file record or it wasn't found</remarks>
        public readonly AttributeBase ChildAttribute;

        /// <summary>
        /// Constructor for AttributeListItem
        /// </summary>
        /// <param name="attributeList">AttributeList holding this item</param>
        public AttributeListItem(AttributeList attributeList)
        {
            Header = attributeList.Body.ToStructure<NTFS_ATTRIBUTE_LIST_HEADER>(attributeList.CurrentOffset);

            if (Header.NameLength > 0)
            {
                Name =
                    Encoding.Unicode.GetString(
                        attributeList.Body.GetBytesAtOffset(attributeList.CurrentOffset + Header.NameOffset,
                            (uint) (Header.NameLength * 2)));
            }

            // TODO: Parse file record if FileRecordNumber is different than parent
            var parentFileRecord = attributeList.Header.FileRecord;

            if (parentFileRecord.Header.MFTRecordNumber == Header.BaseFileReference.FileRecordNumber)
                return;

            if (Header.BaseFileReference.FileRecordNumber < 16)
            {
                var fileRecord = new FileRecord(Header.BaseFileReference.FileRecordNumber,
                    attributeList.Header.FileRecord.Volume);

                if (!fileRecord.Header.Flags.HasFlag(FileRecord.Flags.InUse))
                    throw new InvalidFileRecordException(nameof(FileRecord.Flags), "File record is marked as free",
                        fileRecord);

                if (fileRecord.Header.FileReference == 0)
                    throw new InvalidFileRecordException(nameof(fileRecord.Header.FileReference), "Not a child record",
                        fileRecord);

                ChildAttribute = fileRecord.FindAttribute(Header.AttributeId, Header.Type, attributeList.Header.Name);
            }
            else
            {
                var volume = attributeList.Header.FileRecord.Volume;
                var mftRecord = volume.MFT[0];

                var mftRecordDataAttr = mftRecord.FindAttributeByType(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA);

                byte[] data;

                if (mftRecordDataAttr?.Header is Resident)
                    data = mftRecordDataAttr.Header.ReadBody();
                else if (mftRecordDataAttr?.Header is NonResident)
                {
                    // $DATA attribute in $MFT is huge, so it's best to go to where we want to read and get that cluster, rather than read entire thing
                    var vcn = Header.BaseFileReference.FileRecordNumber * volume.BytesPerFileRecord /
                              (volume.BytesPerSector * volume.SectorsPerCluster);
                    var offsetInCluster = Header.BaseFileReference.FileRecordNumber * volume.BytesPerFileRecord %
                                          (volume.BytesPerSector * volume.SectorsPerCluster);

                    var lcn = ((NonResident) mftRecordDataAttr.Header).VcnToLcn(vcn);

                    if (!lcn.HasValue)
                        return;

                    var cluster = volume.ReadLcn(lcn.Value);

                    data = new byte[volume.BytesPerFileRecord];

                    Array.Copy(cluster.Data, (long) offsetInCluster, data, 0, data.Length);
                }
                else
                {
                    return;
                }

                var fileRecord = new FileRecord(data, volume);
                fileRecord.ReadAttributes();

                ChildAttribute = fileRecord.FindAttribute(Header.AttributeId, Header.Type, Name);
            }

        }

        public struct NTFS_ATTRIBUTE_LIST_HEADER
        {
            public readonly AttributeHeaderBase.NTFS_ATTR_TYPE Type;
            public readonly ushort Length;
            public readonly byte NameLength;
            public readonly byte NameOffset;
            public readonly ulong StartingVcn;
            public readonly Structs.FILE_REFERENCE BaseFileReference;
            public readonly ushort AttributeId;
        }
    }
}
