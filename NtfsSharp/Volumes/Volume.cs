using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NtfsSharp.Data;
using NtfsSharp.Exceptions;
using NtfsSharp.Facades;
using NtfsSharp.Factories.MetaData;
using NtfsSharp.FileRecords;
using NtfsSharp.FileRecords.Attributes;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.Base.NonResident;
using NtfsSharp.MetaData;

namespace NtfsSharp.Volumes
{
    public class Volume : IDisposable, IComparable, IComparable<Volume>, IEquatable<Volume>, IVolume
    {
        public IDiskDriver Driver { get; }

        public BootSector BootSector { get; private set; }
        public IReadOnlyDictionary<uint, FileRecord> MFT { get; private set; }

        public ulong MftLcn { get; private set; }

        public readonly SortedList<uint, FileRecord> FileRecords = new SortedList<uint, FileRecord>();

        #region Units
        /// <summary>
        /// Sectors in a cluster
        /// </summary>
        /// <remarks>Uses the guessed value (from the <seealso cref="IDiskDriver"/>) to read the bootsector, otherwise, the actual value.</remarks>
        public uint SectorsPerCluster => BootSector?.SectorsPerCluster ?? Driver.DefaultSectorsPerCluster;

        /// <summary>
        /// Bytes in a sector
        /// </summary>
        /// <remarks>Uses the guessed value (from the <seealso cref="IDiskDriver"/>) to read the bootsector, otherwise, the actual value.</remarks>
        public ushort BytesPerSector => BootSector?.BytesPerSector ?? Driver.DefaultBytesPerSector;

        /// <summary>
        /// Bytes in a file record
        /// </summary>
        public uint BytesPerFileRecord => BootSector.BytesPerFileRecord;

        /// <summary>
        /// Sectors in a file record
        /// </summary>
        public uint SectorsPerMftRecord => BootSector.SectorsPerMftRecord;
        #endregion

        /// <summary>
        /// Constructor for opening a volume and (optionally) reading it.
        /// </summary>
        /// <param name="diskDriver">Disk driver to use to read the Volume.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="diskDriver"/> is null.</exception>
        /// <remarks>The <seealso cref="Read"/> method is not called when a <see cref="Volume"/> object is created. This must be done by the calling method.</remarks>
        public Volume(BaseDiskDriver diskDriver)
        {
            Driver = diskDriver ?? throw new ArgumentNullException(nameof(diskDriver));
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
        public int CompareTo(IVolume other)
        {
            return other is Volume volume ? CompareTo(volume) : -1;
        }

        public int CompareTo(Volume other)
        {
            if (ReferenceEquals(null, other))
                return -1;

            if (ReferenceEquals(this, other))
                return 0;

            if (BootSector.BootSectorStructure.VolumeSerialNumber == 0)
                return -1;

            if (other.BootSector.BootSectorStructure.VolumeSerialNumber == 0)
                return 1;

            return BootSector.BootSectorStructure.VolumeSerialNumber.CompareTo(other.BootSector.BootSectorStructure.VolumeSerialNumber);
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

        /// <summary>
        /// Reads the boot sector and then the master file table.
        /// </summary>
        /// <returns>Current instance of <seealso cref="Volume"/></returns>
        public IVolume Read()
        {
            return 
                ReadBootSector()
                    .ReadMft();
        }

        /// <summary>
        /// Reads the boot sector (located at offset 0 in the disk)
        /// </summary>
        /// <exception cref="InvalidBootSectorException">Thrown if the bytes per sector, sectors per cluster, or clusters per MFT record is invalid.</exception>
        /// <returns>Current instance of <seealso cref="Volume"/></returns>
        public Volume ReadBootSector()
        {
            BootSector = BootSectorFactory.Build(ReadSectorAtOffset(0).Data);

            return this;
        }

        /// <summary>
        /// Reads the master file table located at the logical cluster number specified in the boot sector.
        /// </summary>
        /// <param name="readMftMirrorOnFailure">If true and an exception occurs reading the MFT specified in the bootsector, the MFT mirror is attempted to be read. (default: true)</param>
        /// <exception cref="InvalidMasterFileTableException">See <seealso cref="MasterFileTable.ReadRecords"/> for conditions causing exception to be thrown.</exception>
        /// <returns>Current instance of <seealso cref="Volume"/></returns>
        public Volume ReadMft(bool readMftMirrorOnFailure = true)
        {
            try
            {
                MFT = ReadMftAtLcn(BootSector.BootSectorStructure.MFTLCN);
                MftLcn = BootSector.BootSectorStructure.MFTLCN;
            }
            catch
            {
                // If readMftMirrorOnFailure and an exception occurred, read from the MFT mirror LCN which is specified in the boot sector.
                if (readMftMirrorOnFailure)
                {
                    MFT = ReadMftAtLcn(BootSector.BootSectorStructure.MFTMirrLCN);
                    MftLcn = BootSector.BootSectorStructure.MFTMirrLCN;
                }
                    
            }

            return this;
        }

        /// <summary>
        /// Reads the master file table located at the specified logical cluster number.
        /// </summary>
        /// <param name="lcn">Logical cluster number to get MFT from.</param>
        /// <remarks>The MFT cannot be returned because <seealso cref="NtfsSharp.FileRecords.Attributes.AttributeList.AttributeListItem"/> relies on the MFT property to get child attributes.</remarks>
        private MasterFileTable ReadMftAtLcn(ulong lcn)
        {
            if (lcn == 0)
                throw new ArgumentOutOfRangeException(nameof(lcn), "Logical cluster number must be greater than 0.");
            
            var masterFileTable = new MasterFileTable(lcn, this);

            masterFileTable.Read();

            return masterFileTable;
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
        /// <param name="readAttributes">Read attributes associated with file record</param>
        /// <returns>List of FileRecord objects</returns>
        public IEnumerable<FileRecord> ReadFileRecords(bool readAttributes)
        {
            // MFT record #5 is root directory
            FileRecords[(uint) MasterFileTable.Files.RootDir] = MFT[(uint) MasterFileTable.Files.RootDir];

            var currentOffset = LcnToOffset(BootSector.BootSectorStructure.MFTLCN) + (ulong) (MFT.Count * BytesPerFileRecord);

            var mftRecord = MFT[0];
            var mftBitmapAttr =
                mftRecord.FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE.BITMAP) as BitmapAttribute;
            var mftDataAttr = mftRecord.FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA) as DataAttribute;

            if (mftBitmapAttr == null)
                throw new Exception("Unable to locate MFT $Bitmap");

            var mftBitmap = mftBitmapAttr.Bitmap;

            var bytesPerFileRecord = SectorsPerMftRecord * BytesPerSector;
            

            for (var currentInode = MFT.Count; currentInode < mftBitmap.Length; currentInode++)
            {
                FileRecord fileRecord = null;

                if (!mftBitmap.Get(currentInode))
                {
                    // Skip to next LCN
                    currentOffset += SectorsPerMftRecord * BytesPerSector;

                    continue;
                }

                try
                {
                    var bytes = (mftDataAttr.Header as NonResident).GetDataAtOffset((ulong) (currentInode * bytesPerFileRecord),
                        bytesPerFileRecord, out uint actualBytesRead);

                    fileRecord = readAttributes
                        ? ReadFileRecordWithAttributes(bytes)
                        : ReadFileRecordWithoutAttributes(bytes);
                }
                catch (InvalidFileRecordException ex)
                {
                    if (ex.ParamName == nameof(FileRecord.FILE_RECORD_HEADER_NTFS.Magic))
                        throw;
                }

                if (fileRecord == null)
                    continue;

                yield return fileRecord;
            }
        }

        /// <summary>
        /// Reads file record with attributes
        /// </summary>
        /// <param name="data">Bytes containing file record</param>
        /// <returns><seealso cref="FileRecord"/> object with attributes</returns>
        private FileRecord ReadFileRecordWithAttributes(byte[] data)
        {
            return FileRecordAttributesFacade.Build(data, this);
        }

        /// <summary>
        /// Reads file record without attributes
        /// </summary>
        /// <param name="data">Bytes containing file record</param>
        /// <returns><seealso cref="FileRecord"/> object with no attributes</returns>
        private FileRecord ReadFileRecordWithoutAttributes(byte[] data)
        {
            return FileRecordFacade.Build(data, this);
        }

        /// <summary>
        /// Reads file record at specified inode
        /// </summary>
        /// <param name="inode">Inode to read</param>
        /// <param name="readAttributes">Read attributes associated with file record</param>
        /// <returns>FileRecord object</returns>
        public FileRecord ReadFileRecord(ulong inode, bool readAttributes)
        {
            var bytesPerFileRecord = SectorsPerMftRecord * BytesPerSector;

            var mftRecord = MFT[0];
            var mftDataAttr =
                mftRecord.FindAttributeByType(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA).Body as DataAttribute;

            var bytes = (mftDataAttr.Header as NonResident).GetDataAtOffset(inode * bytesPerFileRecord,
                        bytesPerFileRecord, out uint actualBytesRead);

            return readAttributes ? ReadFileRecordWithAttributes(bytes) : ReadFileRecordWithoutAttributes(bytes);
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
                if (Driver != null && Driver is IDisposable disposable)
                    disposable.Dispose();
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
