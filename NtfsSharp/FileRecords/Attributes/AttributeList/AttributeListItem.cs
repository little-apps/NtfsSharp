using System.Runtime.InteropServices;
using System.Text;
using NtfsSharp.Exceptions;
using NtfsSharp.FileRecords.Attributes.Base;
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

            var fileRecord = new FileRecord(Header.BaseFileReference.FileRecordNumber,
                attributeList.Header.FileRecord.Volume);

            if (!fileRecord.Header.Flags.HasFlag(FileRecord.Flags.InUse))
                throw new InvalidFileRecordException(nameof(FileRecord.Flags), "File record is marked as free", fileRecord);

            if (fileRecord.Header.FileReference == 0)
                throw new InvalidFileRecordException(nameof(fileRecord.Header.FileReference), "Not a child record", fileRecord);

            ChildAttribute = fileRecord.FindAttribute(Header.AttributeId, Header.Type, attributeList.Header.Name);
        }

        public struct NTFS_ATTRIBUTE_LIST_HEADER
        {
            public readonly AttributeHeader.NTFS_ATTR_TYPE Type;
            public readonly ushort Length;
            public readonly byte NameLength;
            public readonly byte NameOffset;
            public readonly ulong StartingVcn;
            public readonly Structs.FILE_REFERENCE BaseFileReference;
            public readonly ushort AttributeId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly ushort[] Padding;
        }
    }
}
