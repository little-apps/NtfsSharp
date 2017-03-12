using System;
using System.Runtime.InteropServices;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Helpers;

namespace NtfsSharp.FileRecords.Attributes
{
    /// <summary>
    /// A reparse point acts like a miniature "redirector" inside an individual MFT record. The reparse point contains the name of a folder, volumes, or device such as a CD-ROM or DVD. When the MFT record containing the reparse point is opened, the target of the reparse point is opened instead. Using reparse points, it is possible to represent volumes and drives as folders, eliminating the need for additional drive letters and share points.
    /// </summary>
    public class ReparsePoint : AttributeBodyBase
    {
        public readonly NTFS_REPARSE_POINT Data;

        public ReparsePoint(AttributeHeader header) : base(header)
        {
            Data = Body.ToStructure<NTFS_REPARSE_POINT>();
            CurrentOffset += (uint) Marshal.SizeOf<NTFS_REPARSE_POINT>();

            // Data beyond this varies
        }

        public struct NTFS_REPARSE_POINT
        {
            public readonly ReparseFlags Flags;
            public readonly ushort DataLength;
            public readonly ushort Padding;
        }

        [Flags]
        public enum ReparseFlags : uint
        {
            IsAlias = 0x20000000,
            IsHighLatency = 0x40000000,
            IsMicrosoft = 0x80000000,
            NSS = 0x68000005,
            NSSRecover = 0x68000006,
            SIS = 0x68000007,
            DFS = 0x68000008,
            MountPoint = 0x88000003,
            HSM = 0xA8000004,
            SymbolicLink = 0xE8000000
        };
    }
}
