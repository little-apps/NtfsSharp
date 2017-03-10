using NtfsSharp.Helpers;
using System;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;
using static NtfsSharp.PInvoke.Structs;
using System.Text;
using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.FileRecords.Attributes
{
    public class FileName : AttributeBodyBase
    {
        public new static uint HeaderSize => (uint)Marshal.SizeOf<NTFS_ATTR_FILE_NAME>();

        public NTFS_ATTR_FILE_NAME Data { get; private set; }
        public string Filename { get; private set; }

        public FileName(AttributeHeader header) : base(header)
        {
            Data = Bytes.ToStructure<NTFS_ATTR_FILE_NAME>(CurrentOffset);
            CurrentOffset += HeaderSize;

            if (Data.FileNameLength > 0)
            {
                var unicodeLength = Data.FileNameLength * 2;
                Filename = Encoding.Unicode.GetString(Bytes, (int)CurrentOffset, unicodeLength);
                CurrentOffset += (uint)unicodeLength;
            }
                
        }

        [Flags]
        public enum NTFS_FILE_FLAGS : uint
        {
            ReadOnly = 1 << 0,
            Hidden = 1 << 1,
            System = 1 << 2,
            Unused1 = 1 << 3,
            Unused2 = 1 << 4,
            Archive = 1 << 5,
            Device = 1 << 6,
            Normal = 1 << 7,
            Temp = 1 << 8,
            Sparse = 1 << 9,
            Reparse = 1 << 10,
            Compressed = 1 << 11,
            Offline = 1 << 12,
            NotIndexed = 1 << 13,
            Encrypted = 1 << 14,
            // Skip 13 bits
            Directory = 1 << 28,
            IndexView = 1 << 29
        }

        public enum NTFS_NAMESPACE : byte
        {
            Posix = 0,
            Win32 = 1,
            Dos = 2,
            Win32Dos = 3
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct NTFS_ATTR_FILE_NAME
        {
            public readonly FILE_REFERENCE FileReference;
            public readonly FILETIME CreationTime;
            public readonly FILETIME ModifiedTime;
            public readonly FILETIME MFTChangedTime;
            public readonly FILETIME FileReadTime;
            public readonly ulong AllocateSize;
            public readonly ulong RealSize;
            public readonly NTFS_FILE_FLAGS Flags;
            public readonly uint Reparse;
            public readonly byte FileNameLength;
            public readonly NTFS_NAMESPACE Namespace;
        }
    }
}
