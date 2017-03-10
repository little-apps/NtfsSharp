using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Helpers;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace NtfsSharp.FileRecords.Attributes
{
    public class StandardInformation : AttributeBodyBase
    {
        public new MustBe TypeMustBe => MustBe.Resident;

        public static new uint HeaderSize => (uint)Marshal.SizeOf<NTFS_ATTR_STANDARD>();
        public NTFS_ATTR_STANDARD Data { get; private set; }

        public StandardInformation(AttributeHeader header) : base(header)
        {
            Data = Body.ToStructure<NTFS_ATTR_STANDARD>(CurrentOffset);
            CurrentOffset += HeaderSize;
        }

        public struct NTFS_ATTR_STANDARD
        {
            FILETIME CreationTime;
            FILETIME ModifiedTime;
            FILETIME MFTChangedTime;
            FILETIME FileReadTime;
            public readonly uint DosPermissions;
            public readonly uint MaximumVersions;
            public readonly uint VersionNumber;
            public readonly uint ClassID;
            public readonly uint OwnerID;
            public readonly uint SecurityID;
            public readonly ulong QuotaCharged;
            public readonly ulong UpdateSequenceNumber;
        }
    }
}
