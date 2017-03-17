using System.Runtime.InteropServices;
using System.Text;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Helpers;
using NtfsSharp.PInvoke;

namespace NtfsSharp.FileRecords.Attributes.AttributeList
{
    public class AttributeListItem
    {
        public readonly NTFS_ATTRIBUTE_LIST_HEADER Header;
        public readonly string Name;

        public AttributeListItem(uint currentOffset, AttributeList attributeList)
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
