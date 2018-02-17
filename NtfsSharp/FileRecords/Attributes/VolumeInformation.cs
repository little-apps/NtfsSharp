using System;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Helpers;
using System.Runtime.InteropServices;
using NtfsSharp.FileRecords.Attributes.MetaData;

namespace NtfsSharp.FileRecords.Attributes
{
    /// <summary>
    /// Indicates the version and the state of the volume.
    /// </summary>
    [Resident(AttributeHeaderBase.NTFS_ATTR_TYPE.VOLUME_INFORMATION)]
    [NonResident(AttributeHeaderBase.NTFS_ATTR_TYPE.VOLUME_INFORMATION)]
    public sealed class VolumeInformation : AttributeBodyBase
    {
        public static uint HeaderSize => (uint) Marshal.SizeOf<NTFS_ATTR_VOLUME_INFO>();

        public NTFS_ATTR_VOLUME_INFO Data { get; private set; }

        public VolumeInformation(AttributeHeaderBase header) : base(header)
        {
            Data = Body.ToStructure<NTFS_ATTR_VOLUME_INFO>(CurrentOffset);
            CurrentOffset += HeaderSize;
        }

        [Flags]
        public enum VolumeInfoFlags : ushort
        {
            IsDirty = 1 << 0,
            ResizeLogFile = 1 << 1,
            UpgradeOnMount = 1 << 2,
            MountedOnNt4 = 1 << 3,
            DeletingUsn = 1 << 4,
            RepairObjectIds = 1 << 5,
            ModifiedByChkdsk = 1 << 15
        }

        public struct NTFS_ATTR_VOLUME_INFO
        {
            public readonly ulong Empty1;
            public readonly byte MajorVersion;
            public readonly byte MinorVersion;
            public readonly VolumeInfoFlags Flags;
            public readonly uint Empty2;
        }

        public override string ToString()
        {
            return "$VOLUME_INFORMATION (0x70)";
        }
    }
}
