using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NtfsSharp.Data;
using NtfsSharp.Drivers;
using NtfsSharp.Exceptions;
using NtfsSharp.FileRecords;
using NtfsSharp.FileRecords.Attributes;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.Base.NonResident;

namespace NtfsSharp
{
    public class Volume : IDisposable, IComparable, IComparable<Volume>, IEquatable<Volume>
    {
        public readonly BaseDiskDriver Driver;

        public NtfsBootSector BootSector { get; private set; }
        public MasterFileTable MFT { get; private set; }

        public readonly SortedList<uint, FileRecord> FileRecords = new SortedList<uint, FileRecord>();

        #region Units
        public uint SectorsPerCluster = 8;
        public ushort BytesPerSector = 512;
        public uint BytesPerFileRecord = 1024;
        public uint SectorsPerMFTRecord => BytesPerFileRecord / BytesPerSector;
        #endregion

        public Volume(BaseDiskDriver diskDriver, bool doRead = true)
        {
            Driver = diskDriver;

            if (doRead)
                Read();
        }

        ~Volume()
        {
            Dispose(false);
        }

        #region IComparable Implementation
        public int CompareTo(object obj)
        {
            return CompareTo(obj as Volume);
        }
        #endregion

        #region IComparable<Volume> Implementation
        public int CompareTo(Volume other)
        {
            if (ReferenceEquals(null, other))
                return -1;

            if (ReferenceEquals(this, other))
                return 0;

            if (BootSector.VolumeSerialNumber == 0)
                return -1;

            if (other.BootSector.VolumeSerialNumber == 0)
                return 1;

            return BootSector.VolumeSerialNumber.CompareTo(other.BootSector.VolumeSerialNumber);
        }
        #endregion

        #region IEquatable<Volume> Implementation
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj.GetType() == GetType() && Equals((Volume) obj);
        }

        public bool Equals(Volume other)
        {
            return CompareTo(other) == 0;
        }

        public static bool operator ==(Volume left, Volume right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (ReferenceEquals(null, left) || ReferenceEquals(null, right))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(Volume left, Volume right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) SectorsPerCluster;
                hashCode = (hashCode * 397) ^ BytesPerSector.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) BytesPerFileRecord;
                hashCode = (hashCode * 397) ^ BootSector.GetHashCode();
                return hashCode;
            }
        }
        #endregion

        public void Read()
        {
            ReadBootSector();
            ReadMft();
        }

        /// <summary>
        /// Reads the boot sector (located at offset in the disk)
        /// </summary>
        /// <exception cref="InvalidBootSectorException">Thrown if the bytes per sector, sectors per cluster, or clusters per MFT record is invalid.</exception>
        public void ReadBootSector()
        {
            BootSector = ReadLcn(0).ReadFile<NtfsBootSector>(0);

            BytesPerSector = BootSector.BytesPerSector;
            SectorsPerCluster = BootSector.SectorsPerCluster;

            if (BytesPerSector == 0)
                throw new InvalidBootSectorException(nameof(BootSector.BytesPerSector), "BytesPerSector cannot be zero.");

            if (BytesPerSector % 512 != 0)
                throw new InvalidBootSectorException(nameof(BootSector.BytesPerSector), "BytesPerSector must be multiple of 512.");

            if (BytesPerSector > 4096)
                throw new InvalidBootSectorException(nameof(BootSector.BytesPerSector), "BytesPerSector must be equal to or less than 4096.");

            if (SectorsPerCluster == 0)
                throw new InvalidBootSectorException(nameof(BootSector.SectorsPerCluster), "SectorsPerCluster cannot be zero.");

            // If ClustersPerMFTRecord is positive (up to 0x7F), it represents clusters per MFT record
            if (BootSector.ClustersPerMFTRecord <= 0x7F)
                BytesPerFileRecord =
                    (uint)(BootSector.ClustersPerMFTRecord * BootSector.BytesPerSector *
                            BootSector.SectorsPerCluster);
            else
            {
                // Otherwise if it's negative (from 0x80 to 0xFF), the size is 2 raised to its absolute value

                // Anything between 0x80 and 0xE0 will result in an integer overflow (since it's a 32 bit integer)
                if (BootSector.ClustersPerMFTRecord >= 0x80 && BootSector.ClustersPerMFTRecord <= 0xE0)
                    throw new InvalidBootSectorException(nameof(BootSector.ClustersPerMFTRecord), "ClustersPerMFTRecord cannot be between 0xE0 and 0x80");

                BytesPerFileRecord = (uint)(1 << 256 - BootSector.ClustersPerMFTRecord);
            }

        }

        public void ReadMft()
        {
            MFT = new MasterFileTable(this);
            MFT.ReadRecords(BootSector.MFTLCN);
        }
        
        /// <summary>
        /// Gets the total number of inodes aka number of clusters used by the Master File Table.
        /// </summary>
        public uint TotalInodes
        {
            get
            {
                var mftBitmapAttr = MFT[0].FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE.BITMAP) as BitmapAttribute;

                if (mftBitmapAttr == null)
                    throw new Exception("Unable to locate MFT $Bitmap");

                var mftBitmap = mftBitmapAttr.Bitmap;

                return (uint)mftBitmap.Cast<bool>().Count(bit => bit);
            }

        }

        /// <summary>
        /// Produces list of file records
        /// </summary>
        /// <param name="readAttributes">If true, attributes of files are read as well</param>
        /// <returns>List of FileRecord objects</returns>
        public IEnumerable<FileRecord> ReadFileRecords(bool readAttributes)
        {
            // MFT record #5 is root directory
            FileRecords[(uint) MasterFileTable.Files.RootDir] = MFT[(uint) MasterFileTable.Files.RootDir];

            var currentOffset = LcnToOffset(BootSector.MFTLCN) + (ulong) (MFT.Count * BytesPerFileRecord);

            var mftRecord = MFT[0];
            var mftBitmapAttr =
                mftRecord.FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE.BITMAP) as BitmapAttribute;
            var mftDataAttr = mftRecord.FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA) as DataAttribute;

            if (mftBitmapAttr == null)
                throw new Exception("Unable to locate MFT $Bitmap");

            var mftBitmap = mftBitmapAttr.Bitmap;

            var bytesPerFileRecord = SectorsPerMFTRecord * BytesPerSector;

            for (var currentInode = MFT.Count; currentInode < mftBitmap.Length; currentInode++)
            {
                FileRecord fileRecord = null;

                if (!mftBitmap.Get(currentInode))
                {
                    // Skip to next LCN
                    currentOffset += SectorsPerMFTRecord * BytesPerSector;

                    continue;
                }

                try
                {
                    var bytes = (mftDataAttr.Header as NonResident).GetDataAtOffset((ulong) (currentInode * bytesPerFileRecord),
                        bytesPerFileRecord, out uint actualBytesRead);

                    fileRecord = new FileRecord(bytes, this);
                }
                catch (InvalidFileRecordException ex)
                {
                    if (ex.ParamName == nameof(FileRecord.FILE_RECORD_HEADER_NTFS.Magic))
                        throw;
                }

                if (fileRecord == null)
                    continue;

                if (readAttributes)
                    fileRecord.ReadAttributes();

                yield return fileRecord;
            }
        }

        /// <summary>
        /// Reads file record at specified inode
        /// </summary>
        /// <param name="inode">Inode to read</param>
        /// <param name="readAttributes">If true, attributes are read</param>
        /// <returns>FileRecord object</returns>
        public FileRecord ReadFileRecord(ulong inode, bool readAttributes)
        {
            var bytesPerFileRecord = SectorsPerMFTRecord * BytesPerSector;

            var mftRecord = MFT[0];
            var mftDataAttr =
                mftRecord.FindAttributeByType(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA).Body as DataAttribute;

            var bytes = (mftDataAttr.Header as NonResident).GetDataAtOffset((ulong)(inode * bytesPerFileRecord),
                        bytesPerFileRecord, out uint actualBytesRead);

            var fileRecord = new FileRecord(bytes, this);

            if (readAttributes)
                fileRecord.ReadAttributes();

            return fileRecord;
        }

        public Sector ReadSectorAtOffset(ulong offset)
        {
            return new Sector(offset, this);
        }

        public Cluster ReadLcn(ulong lcn)
        {
            return new Cluster(lcn, this);
        }

        public ulong LcnToOffset(ulong lcn)
        {
            return lcn * BytesPerSector * SectorsPerCluster;
        }

        private void ReleaseUnmanagedResources()
        {

        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();

            if (disposing)
            {
                Driver?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region P/Invoke
        public enum Media : byte
        {
            Floppy = 0xf0,
            HardDrive = 0xf8,
            Floppy320K1 = 0xfa,
            Floppy640K = 0xfb,
            Floppy180K = 0xfc,
            Floppy360K = 0xfd,
            Floppy160K = 0xfe,
            Floppy320K2 = 0xff
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NtfsBootSector
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] JMPInstruction;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly char[] OEMID;
            public readonly ushort BytesPerSector;
            public readonly byte SectorsPerCluster;
            public readonly ushort ReservedSectors;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] AlwaysZero1;
            public readonly ushort NotUsed1;
            public readonly Media MediaDescriptor;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public readonly byte[] AlwaysZero2;
            public readonly ushort SectorsPerTrack;
            public readonly ushort NumberOfHeads;
            public readonly uint HiddenSectors;
            public readonly uint NotUsed2;
            public readonly uint NotUsed3;
            public readonly ulong TotalSectors;
            public readonly ulong MFTLCN;
            public readonly ulong MFTMirrLCN;
            public readonly byte ClustersPerMFTRecord;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] NotUsed4;
            public readonly uint ClustersPerIndexBuffer;
            public readonly ulong VolumeSerialNumber;
            public readonly uint NTFSChecksum;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 426)]
            public readonly byte[] BootStrapCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public readonly byte[] Signature;
        }
        #endregion

    }
}
