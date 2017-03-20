using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NtfsSharp.Data;
using NtfsSharp.Exceptions;
using NtfsSharp.FileRecords;

namespace NtfsSharp
{
    public class Volume : IDisposable
    {
        public readonly char Drive;
        public string VolumePath => $@"\\.\{Drive}:";

        public readonly DiskManager Disk;

        public NtfsBootSector BootSector { get; private set; }
        public MasterFileTable MFT { get; private set; }

        public readonly SortedList<uint, FileRecord> FileRecords = new SortedList<uint, FileRecord>();

        #region Units
        public uint SectorsPerCluster = 8;
        public ushort BytesPerSector = 512;
        public uint BytesPerFileRecord = 1024;
        public uint SectorsPerMFTRecord => BytesPerFileRecord / BytesPerSector;
        #endregion

        public Volume(char drive)
        {
            if (!char.IsUpper(drive))
                throw new ArgumentException("Drive letter must be between A and Z", nameof(drive));

            Drive = drive;
            Disk = new DiskManager(VolumePath);

            Read();
        }

        ~Volume()
        {
            Dispose(false);
        }

        private void Read()
        {
            ReadBootSector();
            ReadMft();
            ReadFileRecords();
        }

        private void ReadBootSector()
        {
            BootSector = ReadLcn(0).ReadFile<NtfsBootSector>(0);

            BytesPerSector = BootSector.BytesPerSector;
            SectorsPerCluster = BootSector.SectorsPerCluster;

            // If ClustersPerMFTRecord is positive (up to 0x7F), it represents clusters per MFT record
            if (BootSector.ClustersPerMFTRecord <= 0x7F)
                BytesPerFileRecord =
                    (uint)(BootSector.ClustersPerMFTRecord * BootSector.BytesPerSector *
                            BootSector.SectorsPerCluster);
            else
            {
                // Otherwise if it's negative (from 0x80 to 0xFF), the size is 2 raised to its absolute value
                BytesPerFileRecord = (uint)(1 << 256 - BootSector.ClustersPerMFTRecord);
            }
            
        }

        private void ReadMft()
        {
            MFT = new MasterFileTable(this);
            MFT.ReadRecords();
        }

        public uint TotalInodes
        {
            get
            {
                var mftBitmapAttr =
                MFT[0].FindAttributeByType(AttributeHeader.NTFS_ATTR_TYPE.BITMAP).FirstOrDefault() as BitmapAttribute;

                if (mftBitmapAttr == null)
                    throw new Exception("Unable to locate MFT $Bitmap");

                var mftBitmap = mftBitmapAttr.Bitmap;

                return (uint)mftBitmap.Cast<bool>().Count(bit => bit);
            }
            
        }
        {
            // MFT record #5 is root directory
            FileRecords[5] = MFT[5];

            var shouldContinue = true;
            var currentOffset = LcnToOffset(BootSector.MFTLCN) + (ulong) (MFT.Count * BytesPerFileRecord);

            // Scan until InvalidFileRecordException is thrown (from invalid marker)
            while (shouldContinue)
            {
                try
                {
                    var bytes = new byte[SectorsPerMFTRecord * BytesPerSector];

                    for (var j = 0; j < SectorsPerMFTRecord; j++)
                    {
                        var sector = ReadSectorAtOffset(currentOffset);

                        Array.Copy(sector.Data, 0, bytes, j * BytesPerSector, BytesPerSector);

                        currentOffset += BytesPerSector;
                    }

                    var fileRecord = new FileRecord(bytes, this);
                    fileRecord.ReadAttributes();

                    FileRecords.Add(fileRecord.Header.MFTRecordNumber, fileRecord);
                }
                catch (InvalidFileRecordException ex)
                {
                    if (ex.ParamName == nameof(FileRecord.FILE_RECORD_HEADER_NTFS.Magic))
                        shouldContinue = false;
                }
            }
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
            // TODO release unmanaged resources here
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();

            if (disposing)
            {
                Disk?.Dispose();
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
