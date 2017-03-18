using System;
using System.IO;
using System.Runtime.InteropServices;
using NtfsSharp.Data;
using NtfsSharp.FileRecords;
using NtfsSharp.Helpers;

namespace NtfsSharp
{
    public class Volume : IDisposable
    {
        public readonly char Drive;
        public string VolumePath => $@"\\.\{Drive}:";

        public readonly DiskManager Disk;

        public NtfsBootSector BootSector { get; private set; }
        public MasterFileTable MFT { get; private set; }

        #region Units
        public uint SectorsPerCluster = 8;
        public ushort BytesPerSector = 512;
        public uint BytesPerFileRecord = 1024;
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
