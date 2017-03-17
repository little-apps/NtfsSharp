using System;
using System.Linq;
using System.Runtime.InteropServices;
using NtfsSharp.Exceptions;
using NtfsSharp.Helpers;
using System.Collections.Generic;
using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.FileRecords
{
    /// <summary>
    /// Represents a FILE record
    /// </summary>
    public class FileRecord
    {
        private uint _currentOffset;
        private readonly byte[] _data;
        public readonly Volume Volume;

        public FILE_RECORD_HEADER_NTFS Header { get; private set; }
        public readonly List<AttributeBase> Attributes = new List<AttributeBase>();

        /// <summary>
        /// Reads file record from bytes
        /// </summary>
        /// <param name="data">Bytes with data for file record and attributes</param>
        /// <param name="vol">Volume containing file record</param>
        /// <exception cref="ArgumentNullException">Thrown if Volume is null</exception>
        /// <exception cref="InvalidFileRecordException">Thrown if unable to read file record</exception>
        public FileRecord(byte[] data, Volume vol)
        {
            if (vol == null)
                throw new ArgumentNullException(nameof(vol), "Volume cannot be null");

            Volume = vol;
            _data = data;

            ParseHeader();
        }

        /// <summary>
        /// Reads a file record at specified number in the volume
        /// </summary>
        /// <param name="recordNum">File record number</param>
        /// <param name="vol">Volume containing file record</param>
        /// <exception cref="ArgumentNullException">Thrown if Volume is null</exception>
        /// <exception cref="InvalidFileRecordException">Thrown if unable to read file record</exception>
        public FileRecord(ulong recordNum, Volume vol)
        {
            if (vol == null)
                throw new ArgumentNullException(nameof(vol), "Volume cannot be null");

            vol.Disk.Move(vol.LcnToOffset(vol.BootSector.MFTLCN) + vol.BytesPerFileRecord * recordNum);
            var data = vol.Disk.ReadFile(vol.BytesPerFileRecord);

            Volume = vol;
            _data = data;

            ParseHeader();
        }

        /// <summary>
        /// Parses the file record
        /// </summary>
        /// <exception cref="InvalidFileRecordException">Thrown if magic number is not FILE</exception>
        private void ParseHeader()
        {
            Header = _data.ToStructure<FILE_RECORD_HEADER_NTFS>();
            _currentOffset = (uint)Marshal.SizeOf<FILE_RECORD_HEADER_NTFS>();

            if (!Header.Magic.SequenceEqual(new byte[] { 0x46, 0x49, 0x4C, 0x45 }))
                throw new InvalidFileRecordException(nameof(Header), this);
        }

        /// <summary>
        /// Reads attributes from file record
        /// </summary>
        /// <remarks>Current offset must be set back if calling this more than once</remarks>
        public void ReadAttributes()
        {
            while (_currentOffset < _data.Length && BitConverter.ToUInt32(_data, (int) _currentOffset) != 0xffffffff)
            {
                var newData = new byte[_data.Length - _currentOffset];
                Array.Copy(_data, _currentOffset, newData, 0, newData.Length);

                var attrHeader = AttributeBase.GetAttribute(newData, this);
                Attributes.Add(AttributeBase.ReadBody(attrHeader));

                _currentOffset += attrHeader.Header.Length;

            }
        }

        /// <summary>
        /// Tries to find an attribute in the file record
        /// </summary>
        /// <param name="attrNum">Attribute number</param>
        /// <param name="attrType">Attribute type</param>
        /// <param name="name">Name to match in attribute</param>
        /// <returns>Matching AttributeBase or null if it wasn't found</returns>
        public AttributeBase FindAttribute(ushort attrNum, AttributeHeader.NTFS_ATTR_TYPE attrType, string name)
        {
            var found = false;

            while (_currentOffset < _data.Length && BitConverter.ToUInt32(_data, (int)_currentOffset) != 0xffffffff)
            {
                var newData = new byte[_data.Length - _currentOffset];
                Array.Copy(_data, _currentOffset, newData, 0, newData.Length);

                var attrHeader = AttributeBase.GetAttribute(newData, this);

                if (attrHeader.Header.Type == attrType)
                {
                    if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(attrHeader.Name) &&
                        attrHeader.Header.AttributeID == attrNum)
                        found = true;
                    else if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(attrHeader.Name))
                    {
                        if (name == attrHeader.Name)
                            found = true;
                    }

                }

                _currentOffset += attrHeader.Header.Length;

                if (found)
                    return AttributeBase.ReadBody(attrHeader);
            }

            return null;
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
