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
        private readonly byte[] _data;
        public readonly Volume Volume;

        public FILE_RECORD_HEADER_NTFS Header { get; private set; }
        public readonly List<AttributeBase> Attributes = new List<AttributeBase>();

        public FileRecord(byte[] data, Volume vol)
        {
            if (vol == null)
                throw new ArgumentNullException(nameof(vol), "Volume cannot be null");

            Volume = vol;
            _data = data;

            ParseHeader();
            ReadAttributes();
        }

        private void ParseHeader()
        {
            Header = _data.ToStructure<FILE_RECORD_HEADER_NTFS>();
            _currentOffset = (uint)Marshal.SizeOf<FILE_RECORD_HEADER_NTFS>();

            if (!Header.Magic.SequenceEqual(new byte[] { 0x46, 0x49, 0x4C, 0x45 }))
                throw new InvalidFileRecordException(nameof(Header));
        }

        private void ReadAttributes()
        {
            while (_currentOffset < _data.Length && BitConverter.ToUInt32(_data, (int) _currentOffset) != 0xffffffff)
            {
                var newData = new byte[_data.Length - _currentOffset];
                Array.Copy(_data, _currentOffset, newData, 0, newData.Length);

                var attr = AttributeBase.GetAttribute(newData, this);
                Attributes.Add(attr);

                _currentOffset += attr.Header.Header.Length;

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
