using System.Runtime.InteropServices;

namespace NtfsSharp.Tests.Driver
{
    public class BootSector : BaseDriverCluster
    {
        public NtfsBootSector DummyBootSector;

        protected override bool ShouldGenerateDefault
        {
            get { return false; }
        }

        public BootSector()
        {
            GenerateDefaultDummy();
        }

        protected override void GenerateDefaultDummy()
        {
            DummyBootSector = new NtfsBootSector
            {
                JMPInstruction = new byte[] {0xeb, 0x52, 0x90},
                OEMID = "NTFS    ".ToCharArray(),
                BytesPerSector = (ushort) DummyDriver.BytesPerSector,
                SectorsPerCluster = (byte) DummyDriver.SectorsPerCluster,
                ReservedSectors = 0,
                MediaDescriptor = Volume.Media.HardDrive,
                SectorsPerTrack = 63,
                NumberOfHeads = 255,
                HiddenSectors = 0,
                TotalSectors = DummyDriver.DriveSize / DummyDriver.BytesPerSector,
                MFTLCN = DummyDriver.MasterFileTableLcn,
                MFTMirrLCN = DummyDriver.MasterFileTableLcn,
                ClustersPerMFTRecord = 246,
                ClustersPerIndexBuffer = 1,
                VolumeSerialNumber = 0x1234567890abcdef,
                NTFSChecksum = 0,
                BootStrapCode = new byte[426],
                Signature = new byte[] {0xaa, 0x55}
            };
        }

        public override byte[] Build()
        {
            return StructureToBytes(DummyBootSector, DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NtfsBootSector
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] JMPInstruction;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public char[] OEMID;
            public ushort BytesPerSector;
            public byte SectorsPerCluster;
            public ushort ReservedSectors;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] AlwaysZero1;
            public ushort NotUsed1;
            public Volume.Media MediaDescriptor;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] AlwaysZero2;
            public ushort SectorsPerTrack;
            public ushort NumberOfHeads;
            public uint HiddenSectors;
            public uint NotUsed2;
            public uint NotUsed3;
            public ulong TotalSectors;
            public ulong MFTLCN;
            public ulong MFTMirrLCN;
            public byte ClustersPerMFTRecord;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] NotUsed4;
            public uint ClustersPerIndexBuffer;
            public ulong VolumeSerialNumber;
            public uint NTFSChecksum;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 426)]
            public byte[] BootStrapCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Signature;
        }
    }
}
