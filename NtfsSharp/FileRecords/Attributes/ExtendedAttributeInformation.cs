using System.Runtime.InteropServices;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Helpers;

namespace NtfsSharp.FileRecords.Attributes
{
    /// <summary>
    /// Used to implement under NTFS the HPFS extended attributes used by the information subsystem of OS/2 and OS/2 clients of Windows NT servers. This file attribute may be non-resident because its stream is likely to grow. 
    /// </summary>
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
