using System;
using System.Text;
using NtfsSharp.Exceptions;
using NtfsSharp.Facades;
using NtfsSharp.Factories.FileRecords;
using NtfsSharp.Files.Attributes.Base;
using NtfsSharp.Files.Attributes.Base.NonResident;
using NtfsSharp.Helpers;
using NtfsSharp.PInvoke;

namespace NtfsSharp.Files.Attributes.AttributeList
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
        public readonly Attribute ChildAttribute;

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

            var owner = attributeList.Header.FileRecord.Volume;

            if (Header.BaseFileReference.FileRecordNumber < 16)
            {
                var fileRecord = RecordNumberFactory.Build(Header.BaseFileReference.FileRecordNumber, owner);

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
                var mftRecord = FindMasterFileTableRecord(attributeList);

                var mftRecordDataAttr = mftRecord.FindAttributeByType(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA);

                byte[] data;

                if (mftRecordDataAttr?.Header is Resident)
                    data = mftRecordDataAttr.Header.ReadBody();
                else if (mftRecordDataAttr?.Header is NonResident)
                {
                    // $DATA attribute in $MFT is huge, so it's best to go to where we want to read and get that cluster, rather than read entire thing
                    var bytesPerFileRecord = owner.SectorsPerMftRecord * owner.BytesPerSector;

                    var vcn = Header.BaseFileReference.FileRecordNumber * bytesPerFileRecord /
                              (owner.BytesPerSector * owner.SectorsPerCluster);
                    var offsetInCluster = Header.BaseFileReference.FileRecordNumber * bytesPerFileRecord %
                                          (owner.BytesPerSector * owner.SectorsPerCluster);

                    var lcn = ((NonResident) mftRecordDataAttr.Header).VcnToLcn(vcn);

                    if (!lcn.HasValue)
                        return;

                    var cluster = owner.ReadCluster(lcn.Value);

                    data = new byte[bytesPerFileRecord];

                    Array.Copy(cluster.Data, (long) offsetInCluster, data, 0, data.Length);
                }
                else
                {
                    return;
                }

                var fileRecord = FileRecordAttributesFacade.Build(data, owner);

                ChildAttribute = fileRecord.FindAttribute(Header.AttributeId, Header.Type, Name);
            }

        }
        
        /// <summary>
        /// Finds the $MFT file record to locate attributes from
        /// </summary>
        /// <returns>Returns the <seealso cref="FileRecord"/> representing $MFT or null if not found.</returns>
        private FileRecord FindMasterFileTableRecord(AttributeList parentAttributeList)
        {
            var parentFileRecord = parentAttributeList.Header.FileRecord;

            return parentFileRecord.Volume?.MFT?[0];
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
