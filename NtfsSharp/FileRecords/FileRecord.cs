using System;
using System.Linq;
using System.Runtime.InteropServices;
using NtfsSharp.Exceptions;
using NtfsSharp.Helpers;
using System.Collections.Generic;
using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.FileRecords
{
    public class FileRecord
    {
        private uint _currentOffset;
        private readonly byte[] Data;
        public readonly FILE_RECORD_HEADER_NTFS Header;
        public readonly List<AttributeBase> Attributes = new List<AttributeBase>();

        public FileRecord(byte[] bytes)
        {
            Data = bytes;

            Header = bytes.ToStructure<FILE_RECORD_HEADER_NTFS>();
            _currentOffset = (uint)Marshal.SizeOf<FILE_RECORD_HEADER_NTFS>();

            if (!Header.Magic.SequenceEqual(new byte[] {0x46, 0x49, 0x4C, 0x45}))
                throw new InvalidFileRecordException(nameof(Header));

            ReadAttributes();
        }

        private void ReadAttributes()
        {
            while (_currentOffset < Data.Length && BitConverter.ToUInt32(Data, (int) _currentOffset) != 0xffffffff)
            {
                var newData = new byte[Data.Length - _currentOffset];
                Array.Copy(Data, _currentOffset, newData, 0, newData.Length);

                try
                {
                    var attr = AttributeBase.GetAttribute(newData);
                    Attributes.Add(attr);

                    _currentOffset += attr.Header.Header.Length;
                } catch
                {
                    break;
                }
                
            }
        }

        [Flags]
        public enum Flags : ushort
        {
            InUse = 1 << 0,
            IsDirectory = 1 << 1
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FILE_RECORD_HEADER_NTFS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly byte[] Magic;
            public readonly ushort UpdateSequenceOffset;
            public readonly ushort UpdateSequenceSize;
            public readonly ulong LogFileSequenceNumber;
            public readonly ushort SequenceNumber;
            public readonly ushort HardLinkCount;
            public readonly ushort FirstAttributeOffset;
            public readonly Flags Flags;
            public readonly uint UsedSize;
            public readonly uint AllocateSize;
            public readonly ulong FileReference;
            public readonly ushort NextAttributeID;
            public readonly ushort Align;
            public readonly uint MFTRecordNumber;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public readonly byte[] EndTag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public readonly byte[] FixupArray;
        }
    }
}
