using System;
using System.Runtime.InteropServices;
using NtfsSharp.Drivers.Physical.Exceptions;
using NtfsSharp.Helpers;

namespace NtfsSharp.Drivers.Physical
{
    public class MasterBootRecord
    {
        /// <summary>
        /// The maximum of partitions in a MBR
        /// </summary>
        public const ushort MaxMasterBootRecordPartitions = 4;

        /// <summary>
        /// The size of a Logical Block Size (in bytes)
        /// </summary>
        /// <remarks>Unlike a sector, a LBA is always 512 bytes.</remarks>
        public const uint LogicalBlockAddressSize = 512;

        private readonly BaseDiskDriver _diskDriver;
        
        public MasterBootRecordStruct MbrStruct { get; private set; }

        /// <summary>
        /// The Guid Partition Table of the drive.
        /// </summary>
        /// <remarks>Not all drives use a GPT so this maybe null.</remarks>
        public GuidPartitionTable GuidPartitionTable { get; private set; }

        public MasterBootRecord(BaseDiskDriver diskDriver)
        {
            _diskDriver = diskDriver ?? throw new ArgumentNullException(nameof(diskDriver));

            ReadMasterBootRecord();
            ReadGuidPartitionTable();
        }

        /// <summary>
        /// Reads the MBR from the beginning of the physical drive
        /// </summary>
        /// <exception cref="InvalidMasterBootRecord">Thrown if sector marker isn't 0xAA55</exception>
        private void ReadMasterBootRecord()
        {
            _diskDriver.Move(0);

            var mbrBytes = _diskDriver.ReadSectorBytes(LogicalBlockAddressSize);
            MbrStruct = mbrBytes.ToStructure<MasterBootRecordStruct>();

            if (MbrStruct.EndOfSectorMarker != 0xAA55)
                throw new InvalidMasterBootRecord("End of sector marker does not match 0xAA55",
                    nameof(MbrStruct.EndOfSectorMarker));
        }

        /// <summary>
        /// If a Guid Partition Table exists in the MBR, it is read
        /// </summary>
        private void ReadGuidPartitionTable()
        {
            if (MbrStruct.PartitionEntries[0].SystemID != SystemId.GptProtectiveMbr)
                return;

            GuidPartitionTable = new GuidPartitionTable(this, _diskDriver);
        }

        /// <summary>
        /// Selects the partition using the MBR or GPT
        /// </summary>
        /// <param name="partition">Index of parition to read.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the partition index is greater than 4 (if using MBR) or 128 (if using GPT)</exception>
        public Partition SelectPartition(uint partition)
        {
            if (GuidPartitionTable == null)
            {
                if (partition >= MbrStruct.PartitionEntries.Length)
                    throw new ArgumentOutOfRangeException(nameof(partition),
                        $"Partition number cannot be greater than or equal to {MbrStruct.PartitionEntries.Length}");

                var partitionEntry = MbrStruct.PartitionEntries[partition];
                
                return new Partition(partitionEntry.RelativeSector, partitionEntry.RelativeSector + partitionEntry.TotalSectors, this);
            }
            else
            {
                if (partition >= GuidPartitionTable.Count)
                    throw new ArgumentOutOfRangeException(nameof(partition),
                        $"Partition number cannot be greater than or equal to {GuidPartitionTable.Count}");

                var efiPartitionEntry = GuidPartitionTable[(int) partition];

                return new Partition((uint) efiPartitionEntry.FirstLba, efiPartitionEntry.LastLba, this);
            }
        }

        /// <summary>
        /// Moves to logical block address
        /// </summary>
        /// <param name="lba">Logical Block Address</param>
        /// <returns>New offset on disk</returns>
        public long MoveToLba(ulong lba)
        {
            return _diskDriver.Move((long) LbaToOffset(lba));
        }

        /// <summary>
        /// Gets the offset on the disk of the logical block address
        /// </summary>
        /// <param name="lba">Logical Block Address</param>
        /// <returns>Offset on hard drive</returns>
        public static ulong LbaToOffset(ulong lba)
        {
            return lba * LogicalBlockAddressSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MasterBootRecordStruct
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 446)]
            public readonly byte[] BootCode;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly PartitionEntry[] PartitionEntries;

            public readonly ushort EndOfSectorMarker;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PartitionEntry
        {
            public readonly PhysicalDiskDriver.BootIndicator BootIndicator;
            public readonly byte StartingHead;
            public readonly ushort StartingSectCylinder;   // Need Bit fields
            public readonly SystemId SystemID;
            public readonly byte EndingHead;
            public readonly ushort EndingSectCylinder;     // Need Bit fields
            public readonly uint RelativeSector;
            public readonly uint TotalSectors;
        }

        public enum SystemId : byte
        {
            Empty = 0x00,
            Fat12 = 0x01,
            XenixRoot = 0x02,
            XenixUsr = 0x03,
            Fat16Inf32Mb = 0x04,
            Extended = 0x05,
            Fat16 = 0x06,
            NtfsHpfs = 0x07,
            Aix = 0x08,
            AixBoot = 0x09,
            Os2BootMgr = 0x0a,
            PriFat32Int13 = 0x0b,
            ExtFat32Int13 = 0x0c,
            SiliconSafe = 0x0d,
            ExtFat16Int13 = 0x0e,
            Win95ExtPartition = 0x0f,
            Opus = 0x10,
            Fat12Hidden = 0x11,
            CompaqDiag = 0x12,
            Fat16HiddenInf32Mb = 0x14,
            Fat16Hidden = 0x16,
            NtfsHpfsHidden = 0x17,
            AstSmartsleepPartition = 0x18,
            Osr2Fat32 = 0x1b,
            Osr2Fat32Lba = 0x1c,
            HiddenFat16Lba = 0x1e,
            NecDos = 0x24,
            PqserviceRouterboot = 0x27,
            AtheosFileSystem = 0x2a,
            Nos = 0x32,
            JfsOnOs2OrEcs = 0x35,
            Theos_2Gb = 0x38,
            Plan9TheosSpanned = 0x39,
            Theos_4Gb = 0x3a,
            TheosExtended = 0x3b,
            PartitionmagicRecovery = 0x3c,
            HiddenNetware = 0x3d,
            Venix = 0x40,
            LinuxPpcPrep = 0x41,
            LinuxSwap = 0x42,
            LinuxNative = 0x43,
            Goback = 0x44,
            BootUsEumelElan = 0x45,
            EumelElan1 = 0x46,
            EumelElan2 = 0x47,
            EumelElan3 = 0x48,
            Oberon = 0x4c,
            Qnx4X = 0x4d,
            Qnx4X_2NdPart = 0x4e,
            Qnx4X_3RdPartOberon = 0x4f,
            OntrackLynxOberon = 0x50,
            OntrackNovell = 0x51,
            CpMMicroportSysvAt = 0x52,
            DiskManagerAux3 = 0x53,
            DiskManagerDdo = 0x54,
            EzDrive = 0x55,
            GoldenBowEzBios = 0x56,
            DriveproVndi = 0x57,
            PriamEdisk = 0x5c,
            Speedstor = 0x61,
            GnuHurd = 0x63,
            Novel1 = 0x64,
            Netware386 = 0x65,
            NetwareSmsPartition = 0x66,
            Novell1 = 0x67,
            Novell2 = 0x68,
            NetwareNss = 0x69,
            DisksecureMultiBoot = 0x70,
            V7X86 = 0x72,
            PcIx = 0x75,
            M2FsM2CsVndi = 0x77,
            XoslFs = 0x78,
            MinuxOld = 0x80,
            MinuxLinux = 0x81,
            LinuxSwap2 = 0x82,
            LinuxNative2 = 0x83,
            Os2HiddenHibernation = 0x84,
            LinuxExtended = 0x85,
            OldLinuxRaidFat16 = 0x86,
            NtfsVolumeSet = 0x87,
            LinuxPlaintextTable = 0x88,
            LinuxKernelAirBoot = 0x8a,
            FaultTolerantFat32 = 0x8b,
            FaultTolerantFat32Int13H = 0x8c,
            FreeFdiskFat12 = 0x8d,
            LinuxLogicalVolumeManager = 0x8e,
            FreeFdiskPrimaryFat16 = 0x90,
            FreeFdiskExtended = 0x91,
            FreeFdiskLargeFat16 = 0x92,
            Amoeba = 0x93,
            AmoebaBbt = 0x94,
            MitExopc = 0x95,
            ChrpIso9660 = 0x96,
            FreeFdiskFat32 = 0x97,
            FreeFdiskFat32Lba = 0x98,
            Dce376 = 0x99,
            FreeFdiskFat16Lba = 0x9a,
            FreeFdiskExtendedLba = 0x9b,
            Forthos = 0x9e,
            BsdOs = 0x9f,
            LaptopHibernation = 0xa0,
            LaptopHibernationHp = 0xa1,
            HpExpansionSpeedstor1 = 0xa3,
            HpExpansionSpeedstor2 = 0xa4,
            Bsd386 = 0xa5,
            OpenbsdSpeedstor = 0xa6,
            Nextstep = 0xa7,
            MacOsX = 0xa8,
            Netbsd = 0xa9,
            Olivetti = 0xaa,
            MacOsXBootGo = 0xab,
            RiscOsAdfs = 0xad,
            Shagos = 0xae,
            ShagosSwapMacosXHfs = 0xaf,
            BootstarDummy = 0xb0,
            HpExpansionQnx = 0xb1,
            QnxPowerSafe = 0xb2,
            HpExpansionQnx2 = 0xb3,
            HpExpansionSpeedstor3 = 0xb4,
            HpExpansionFat16 = 0xb6,
            BsdiFs = 0xb7,
            BsdiSwap = 0xb8,
            BootWizardHidden = 0xbb,
            AcronisBackup = 0xbc,
            Bonnydos286 = 0xbd,
            Solaris8Boot = 0xbe,
            NewSolaris = 0xbf,
            CtosReal32DrDos = 0xc0,
            DrdosSecured = 0xc1,
            HiddenLinuxSwap = 0xc3,
            DrdosSecuredFat16 = 0xc4,
            DrdosSecuredExtended = 0xc5,
            DrdosSecuredFat16Stripe = 0xc6,
            Syrinx = 0xc7,
            DrDos81 = 0xc8,
            DrDos82 = 0xc9,
            DrDos83 = 0xca,
            DrDos7SecuredFat32Chs = 0xcb,
            DrDos7SecuredFat32Lba = 0xcc,
            CtosMemdump = 0xcd,
            DrDos7Fat16X = 0xce,
            DrDos7SecuredExtDos = 0xcf,
            Real32Secure = 0xd0,
            OldMultiuserFat12 = 0xd1,
            OldMultiuserFat16 = 0xd4,
            OldMultiuserExtended = 0xd5,
            OldMultiuserFat162 = 0xd6,
            CpM86 = 0xd8,
            NonFsDataPowercopyBackup = 0xda,
            CpM = 0xdb,
            HiddenCtosMemdump = 0xdd,
            DellPoweredgeUtil = 0xde,
            DgUxDiskManagerBootit = 0xdf,
            AccessDos = 0xe1,
            DosRO = 0xe3,
            SpeedstorFat16Extended = 0xe4,
            StorageDimensionsSpeedstor = 0xe6,
            Luks = 0xe8,
            RufusExtraFreedesktop = 0xea,
            BeosBfs = 0xeb,
            SkyosSkyfs = 0xec,
            GptHybridMbr = 0xed,
            GptProtectiveMbr = 0xee,
            EfiSystemPartition = 0xef,
            LinuxPaRiscBoot = 0xf0,
            StorageDimensionsSpeedstor2 = 0xf1,
            DosSecondary = 0xf2,
            SpeedstorLargePrologue = 0xf4,
            PrologueMultiVolume = 0xf5,
            StorageDimensionsSpeedstor3 = 0xf6,
            DdrdriveSolidStateFs = 0xf7,
            Pcache = 0xf9,
            Bochs = 0xfa,
            VmwareFileSystem = 0xfb,
            VmwareSwap = 0xfc,
            LinuxRaid = 0xfd,
            SpeedstorLanstepLinux = 0xfe,
            Bbt = 0xff
        }
    }
}
