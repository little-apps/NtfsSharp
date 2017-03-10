using System;
using System.Runtime.InteropServices;
using System.Text;
using NtfsSharp.Helpers;
using NtfsSharp.PInvoke;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace NtfsSharp.FileRecords.Attributes.Shared
{
    public class FileName
    {
        public static uint HeaderSize => (uint)Marshal.SizeOf<NTFS_ATTR_FILE_NAME>();

        public uint CurrentOffset = 0;

        public NTFS_ATTR_FILE_NAME Data { get; private set; }
        public string Filename { get; private set; }

        public FileName(byte[] bytes, uint startOffset = 0)
        {
            CurrentOffset = startOffset;

            Data = bytes.ToStructure<NTFS_ATTR_FILE_NAME>(CurrentOffset);
            CurrentOffset += (uint) Marshal.SizeOf<NTFS_ATTR_FILE_NAME>();

            if (Data.FileNameLength <= 0)
                return;

            var unicodeLength = Data.FileNameLength * 2;
            Filename = Encoding.Unicode.GetString(bytes, (int) CurrentOffset, unicodeLength);
            CurrentOffset += (uint)unicodeLength;
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
            public readonly Structs.FILE_REFERENCE FileReference;
            public readonly System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            public readonly System.Runtime.InteropServices.ComTypes.FILETIME ModifiedTime;
            public readonly System.Runtime.InteropServices.ComTypes.FILETIME MFTChangedTime;
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
