using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using NtfsSharp.FileRecords.Attributes.Base;

namespace NtfsSharp.Tests.Driver
{
    public class DummyFileRecord
    {
        public FILE_RECORD_HEADER_NTFS FileRecord;
        public readonly List<AttributeBase> Attributes = new List<AttributeBase>();

        /// <summary>
        /// Constructor for DummyFileRecord
        /// </summary>
        /// <param name="fileRecord">File record structure</param>
        /// <param name="attributes">Any attributes to use with file record. If null, the offset of the first attribute is 0xffffffff. (default: null)</param>
        public DummyFileRecord(FILE_RECORD_HEADER_NTFS fileRecord, List<AttributeBase> attributes = null)
        {
            FileRecord = fileRecord;

            if (attributes != null)
                Attributes.AddRange(attributes);
        }

        /// <summary>
        /// Builds the file record in it's byte format (without update sequence array)
        /// </summary>
        /// <param name="fileRecordSize">Size of the file record in bytes</param>
        /// <param name="dummyDriver">Dummy driver instance</param>
        /// <returns>File record in bytes</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="DummyDriver"/> is null.</exception>
        public byte[] Build(uint fileRecordSize, DummyDriver dummyDriver)
        {
            if (dummyDriver == null)
                throw new ArgumentNullException(nameof(dummyDriver), "Dummy driver cannot be null.");

            var bytes = new byte[fileRecordSize];
            var ptr = Marshal.AllocHGlobal((int)fileRecordSize);

            for (var i = 0; i < fileRecordSize; i+=8)
            {
                Marshal.WriteInt64(ptr, i, 0);
            }
            
            Marshal.StructureToPtr(FileRecord, ptr, true);

            Marshal.Copy(ptr, bytes, 0, (int)fileRecordSize);

            InsertAttributes(bytes, fileRecordSize - Marshal.SizeOf<FILE_RECORD_HEADER_NTFS>(), dummyDriver);

            Marshal.FreeHGlobal(ptr);

            return bytes;
        }

        /// <summary>
        /// Builds the file record in it's byte format and adds in the update sequence array
        /// </summary>
        /// <param name="fileRecordSize">Size of the file record in bytes</param>
        /// <param name="dummyDriver">Dummy driver instance</param>
        /// <param name="endTag">End tag to be inserted at end of each sector in file record</param>
        /// <param name="fixups">
        ///     Fixup bytes that will be used to be replaced at end of each sector in file record. 
        ///     If null, uses existing 2 bytes at end of each sector for fixups.
        /// </param>
        /// <returns>File record with update sequence array added in</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="DummyDriver"/> is null.</exception>
        public byte[] BuildWithUsa(uint fileRecordSize, DummyDriver dummyDriver, ushort endTag, ushort[] fixups = null)
        {
            if (dummyDriver == null)
                throw new ArgumentNullException(nameof(dummyDriver), "Dummy driver cannot be null.");

            var bytes = Build(fileRecordSize, dummyDriver);

            if (fixups == null)
            {
                fixups = new ushort[fileRecordSize / 512];

                for (var i = 0; i < fixups.Length; i++)
                {
                    fixups[i] = BitConverter.ToUInt16(bytes, (i + 1) * 512 - 2);
                }
            }

            InsertUsa(bytes, endTag, fixups);

            return bytes;
        }

        /// <summary>
        /// Inserts attributes into file record bytes
        /// </summary>
        /// <param name="bytes">File record with attributes added</param>
        /// <remarks>If no attributes are specified, the first attribute offset is 0xffffffff</remarks>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="DummyDriver"/> is null.</exception>
        private void InsertAttributes(byte[] bytes, long bytesLeft, DummyDriver dummyDriver)
        {
            if (dummyDriver == null)
                throw new ArgumentNullException(nameof(dummyDriver), "Dummy driver cannot be null.");

            if (Attributes.Count == 0)
            {
                var endAttributes = BitConverter.GetBytes(uint.MaxValue);
                Array.Copy(endAttributes, 0, bytes, FileRecord.FirstAttributeOffset, endAttributes.Length);

                return;
            }
            
            // TODO: Add in any attributes
        }

        /// <summary>
        /// Inserts update sequence array into the file record bytes
        /// </summary>
        /// <param name="bytes">File record bytes</param>
        /// <param name="endTag">End tag to be set at end of each sector in file record</param>
        /// <param name="fixups">Bytes that are to replace end tags</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the # of sectors in file record does not fit the number of fixups</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the # of fixups does not fit the size of the update sequency array minus one</exception>
        private static void InsertUsa(byte[] bytes, ushort endTag, ushort[] fixups)
        {
            var usaOffset = BitConverter.ToUInt16(bytes, 4);
            var usaSize = BitConverter.ToUInt16(bytes, 6);

            if (bytes.Length / 512 != usaSize - 1)
                throw new ArgumentOutOfRangeException(nameof(bytes), $"Must be {(usaSize - 1) * 512} bytes");

            if (fixups.Length < usaSize - 1)
                throw new ArgumentOutOfRangeException(nameof(fixups),
                    $"End tags must have at least {usaSize - 1} elements");

            var endTagBytes = new byte[0];
            
            for (var i = 0; i < usaSize; i++)
            {
                if (i == 0)
                {
                    endTagBytes = BitConverter.GetBytes(endTag);
                    Array.Copy(endTagBytes, 0, bytes, usaOffset, endTagBytes.Length);
                    continue;
                }

                var fixup = fixups[i - 1];
                var fixupBytes = BitConverter.GetBytes(fixup);

                // Copy to update sequence array after FILE record
                Array.Copy(fixupBytes, 0, bytes, usaOffset + i * 2, fixupBytes.Length);
                // Copy end tags to end of corresponding sector
                Array.Copy(endTagBytes, 0, bytes, i * 512 - 2, fixupBytes.Length);
            }
        }

        public static DummyFileRecord BuildDummyFileRecord(uint mftRecordNum)
        {
            return new DummyFileRecord(new FILE_RECORD_HEADER_NTFS
            {
                Magic = Encoding.ASCII.GetBytes("FILE"),
                UpdateSequenceOffset = 48,
                UpdateSequenceSize = 3,
                LogFileSequenceNumber = 0,
                SequenceNumber = 0,
                HardLinkCount = 1,
                FirstAttributeOffset = 56,
                Flags = Flags.InUse,
                UsedSize = 56,
                AllocateSize = 1024,
                FileReference = 0,
                NextAttributeID = 0,
                Align = 0,
                MFTRecordNumber = mftRecordNum
            });
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
            public byte[] Magic;
            public ushort UpdateSequenceOffset;
            public ushort UpdateSequenceSize;
            public ulong LogFileSequenceNumber;
            public ushort SequenceNumber;
            public ushort HardLinkCount;
            public ushort FirstAttributeOffset;
            public Flags Flags;
            public uint UsedSize;
            public uint AllocateSize;
            public ulong FileReference;
            public ushort NextAttributeID;
            public ushort Align;
            public uint MFTRecordNumber;
        }
    }
}
