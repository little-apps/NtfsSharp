﻿using System.IO;
using System.Runtime.InteropServices;
using NtfsSharp.Files.Attributes.Base;
using NtfsSharp.Files.Attributes.MetaData;
using NtfsSharp.Helpers;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace NtfsSharp.Files.Attributes
{
    /// <summary>
    /// Includes information such as timestamp and link count.
    /// </summary>
    [Resident(AttributeHeaderBase.NTFS_ATTR_TYPE.STANDARD_INFORMATION)]
    public sealed class StandardInformation : AttributeBodyBase
    {
        public static uint HeaderSize => (uint)Marshal.SizeOf<NTFS_ATTR_STANDARD>();
        public NTFS_ATTR_STANDARD Data { get; private set; }

        public StandardInformation(AttributeHeaderBase header) : base(header)
        {
            Data = Body.ToStructure<NTFS_ATTR_STANDARD>(CurrentOffset);
            CurrentOffset += HeaderSize;
        }

        public struct NTFS_ATTR_STANDARD
        {
            public FILETIME CreationTime;
            public FILETIME ModifiedTime;
            public FILETIME MFTChangedTime;
            public FILETIME FileReadTime;
            public readonly FileAttributes DosPermissions;
            public readonly uint MaximumVersions;
            public readonly uint VersionNumber;
            public readonly uint ClassID;
            public readonly uint OwnerID;
            public readonly uint SecurityID;
            public readonly ulong QuotaCharged;
            public readonly ulong UpdateSequenceNumber;
        }

        public override string ToString()
        {
            return "$STANDARD_INFORMATION (0x10)";
        }
    }
}
