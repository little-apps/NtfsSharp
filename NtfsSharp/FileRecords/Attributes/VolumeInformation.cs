using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Helpers;
using System.Runtime.InteropServices;

namespace NtfsSharp.FileRecords.Attributes
{
    /// <summary>
    /// Indicates the version and the state of the volume. 
    /// </summary>
    public class VolumeInformation : AttributeBodyBase
    {
        public static uint HeaderSize => (uint)Marshal.SizeOf<NTFS_ATTR_VOLUME_INFO>();

        public NTFS_ATTR_VOLUME_INFO Data { get; private set; }

        public VolumeInformation(AttributeHeader header) : base(header)
        {
            Data = Body.ToStructure<NTFS_ATTR_VOLUME_INFO>(CurrentOffset);
            CurrentOffset += HeaderSize;
        }

        public struct NTFS_ATTR_VOLUME_INFO
        {
            public readonly ulong Empty1;
            public readonly byte MajorVersion;
            public readonly byte MinorVersion;
            public readonly ushort Flags;
            public readonly uint Empty2;
        }
    }
}
