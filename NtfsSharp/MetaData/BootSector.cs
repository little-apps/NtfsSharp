using System.Runtime.InteropServices;

namespace NtfsSharp.MetaData
{
    public class BootSector
    {
        /// <summary>
        /// Structure containing meta data for NTFS volume
        /// </summary>
        public NtfsBootSector BootSectorStructure { get; }

        /// <summary>
        /// Bytes in a sector
        /// </summary>
        public ushort BytesPerSector { get; set; }

        /// <summary>
        /// Sectors in a cluster
        /// </summary>
        public uint SectorsPerCluster { get; set; }

        /// <summary>
        /// Bytes in a file record
        /// </summary>
        public uint BytesPerFileRecord { get; set; }

        /// <summary>
        /// Sectors in a file record
        /// </summary>
        public uint SectorsPerMftRecord => BytesPerFileRecord / BytesPerSector;

        /// <summary>
        /// Constructor for BootSector
        /// </summary>
        /// <param name="ntfsBootSector">Boot sector structure</param>
        /// <remarks>Use <see cref="Factories.MetaData.BootSectorFactory"/> to create this object.</remarks>
        public BootSector(NtfsBootSector ntfsBootSector)
        {
            BootSectorStructure = ntfsBootSector;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NtfsBootSector
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] JMPInstruction;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] OEMID;
            public readonly ushort BytesPerSector;
            public readonly byte SectorsPerCluster;
            public readonly ushort ReservedSectors;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] AlwaysZero1;
            public readonly ushort NotUsed1;
            public readonly Volume.Media MediaDescriptor;
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
    }
}
