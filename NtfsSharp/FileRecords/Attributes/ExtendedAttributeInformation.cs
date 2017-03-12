using System.Runtime.InteropServices;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Helpers;

namespace NtfsSharp.FileRecords.Attributes
{
    public class ExtendedAttributeInformation : AttributeBodyBase
    {
        public readonly NTFS_EA_INFORMATION Data;

        public ExtendedAttributeInformation(AttributeHeader header) : base(header)
        {
            Data = Body.ToStructure<NTFS_EA_INFORMATION>();
            CurrentOffset += (uint) Marshal.SizeOf<NTFS_EA_INFORMATION>();
        }

        public struct NTFS_EA_INFORMATION
        {
            public readonly ushort PackedSize;
            public readonly ushort NeedEASize;
            public readonly uint UnpackedSize;
        }
    }
}
