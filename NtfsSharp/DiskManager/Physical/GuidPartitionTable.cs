using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using NtfsSharp.DiskManager.Physical.Exceptions;
using NtfsSharp.Helpers;

namespace NtfsSharp.DiskManager.Physical
{
    public class GuidPartitionTable : IReadOnlyCollection<GuidPartitionTable.EfiPartitionEntry>
    {
        private const uint HeaderLba = 1;

        private PhysicalDiskManager DiskManager { get; }

        public PartitionTableHeader Header { get; }

        private ReadOnlyCollection<EfiPartitionEntry> Table { get; set; }

        public int Count
        {
            get { return Table.Count; }
        }

        public EfiPartitionEntry this[int index] => Table[index];

        /// <summary>
        /// Constructor for GuidPartitionTable
        /// </summary>
        /// <param name="diskManager">Instance of <see cref="PhysicalDiskManager"/> containing the GPT</param>
        /// <exception cref="InvalidGuidPartitionTable">Thrown if the GPT signature is not 'EFI PART'</exception>
        public GuidPartitionTable(PhysicalDiskManager diskManager)
        {
            DiskManager = diskManager;

            DiskManager.MoveToLba(HeaderLba);

            var partitionTableHeaderBytes = DiskManager.ReadFile(PhysicalDiskManager.LogicalBlockAddressSize);
            Header = partitionTableHeaderBytes.ToStructure<PartitionTableHeader>();

            if (Header.Signature != 0x5452415020494645) // 'EFI PART'
                throw new InvalidGuidPartitionTable("The GPT signature is not valid", nameof(Header.Signature));

            ReadPartitionEntries();
        }

        /// <summary>
        /// Reads the partition table
        /// </summary>
        private void ReadPartitionEntries()
        {
            var partitionEntries = new List<EfiPartitionEntry>();
            var sectorBytes = new byte[1];

            for (var offset = 0; offset < Header.PartitionEntries * Header.PartitionEntrySize; offset += (int)Header.PartitionEntrySize)
            {
                if (offset % PhysicalDiskManager.LogicalBlockAddressSize == 0)
                {
                    // If offset is a multiple of 512, we're reading a new sector
                    var currentLba = Header.PartitionEntriesStartLba +
                                     (ulong)(offset / PhysicalDiskManager.LogicalBlockAddressSize);
                    DiskManager.MoveToLba(currentLba);

                    sectorBytes = DiskManager.ReadFile(PhysicalDiskManager.LogicalBlockAddressSize);
                }

                var partitionEntryBytes = new byte[Header.PartitionEntrySize];
                Array.Copy(sectorBytes, offset, partitionEntryBytes, 0, Header.PartitionEntrySize);

                // If the first 8 bytes are 0, it's the end of the partitions
                if (BitConverter.ToUInt64(partitionEntryBytes, 0) == 0)
                    break;

                var partitionEntry = partitionEntryBytes.ToStructure<EfiPartitionEntry>();
                partitionEntries.Add(partitionEntry);
            }

            Table = new ReadOnlyCollection<EfiPartitionEntry>(partitionEntries);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PartitionTableHeader
        {
            public readonly ulong Signature;
            public readonly uint Revision;
            public readonly uint HeaderSize;
            public readonly uint Crc32Checksum;
            public readonly uint Reserved;
            public readonly ulong CurrentLba;
            public readonly ulong BackupLba;
            public readonly ulong FirstUsableLba;
            public readonly ulong LastUsableLba;
            public readonly Guid DiskGuid;
            public readonly ulong PartitionEntriesStartLba;
            public readonly uint PartitionEntries;
            public readonly uint PartitionEntrySize;
            public readonly uint PartitionArrayCrc32Checksum;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct EfiPartitionEntry
        {
            // First 4 bytes of GUID represent the type
            [FieldOffset(0)]
            public readonly EfiType Type;
            [FieldOffset(0)]
            public readonly Guid TypeGuid;
            [FieldOffset(16)]
            public readonly Guid PartitionGuid;
            [FieldOffset(32)]
            public readonly ulong FirstLba;
            [FieldOffset(40)]
            public readonly ulong LastLba;
            [FieldOffset(48)]
            public readonly GuidFlags Flags;
        }

        public enum EfiType : uint
        {
            Unused = 0x00000000,
            Mbr = 0x024DEE41,
            System = 0xC12A7328,
            BiosBoot = 0x21686148,
            Iffs = 0xD3BFE2DE,
            SonyBoot = 0xF4019732,
            LenovoBoot = 0xBFBFAFE7,
            Msr = 0xE3C9E316,
            BasicData = 0xEBD0A0A2,
            LdmMeta = 0x5808C8AA,
            Ldm = 0xAF9B60A0,
            Recovery = 0xDE94BBA4,
            Gpfs = 0x37AFFC90,
            StorageSpaces = 0xE75CAF8F,
            HpuxData = 0x75894C1E,
            HpuxService = 0xE2A1E728,
            LinuxDaya = 0x0FC63DAF,
            LinuxRaid = 0xA19D880F,
            LinuxRoot32 = 0x44479540,
            LinuxRoot64 = 0x4F68BCE3,
            LinuxRootArm32 = 0x69DAD710,
            LinuxRootArm64 = 0xB921B045,
            LinuxSwap = 0x0657FD6D,
            LinuxLvm = 0xE6D6D379,
            LinuxHome = 0x933AC7E1,
            LinuxSrv = 0x3B8F8425,
            LinuxDmCrypt = 0x7FFEC5C9,
            LinuxLuks = 0xCA7D7CCB,
            LinuxReserved = 0x8DA63339,
            FreebsdBoot = 0x83BD6B9D,
            FreebsdData = 0x516E7CB4,
            FreebsdSwap = 0x516E7CB5,
            FreebsdUfs = 0x516E7CB6,
            FreebsdVinum = 0x516E7CB8,
            FreebsdZfs = 0x516E7CBA,
            OsxHfs = 0x48465300,
            OsxUfs = 0x55465300,
            OsxZfs = 0x6A898CC3,
            OsxRaid = 0x52414944,
            OsxRaidOffline = 0x52414944,
            OsxRecovery = 0x426F6F74,
            OsxLabel = 0x4C616265,
            OsxTvRecovery = 0x5265636F,
            OsxCoreStorage = 0x53746F72,
            SolarisBoot = 0x6A82CB45,
            SolarisRoot = 0x6A85CF4D,
            SolarisSwap = 0x6A87C46F,
            SolarisBackup = 0x6A8B642B,
            SolarisUsr = 0x6A898CC3,
            SolarisVar = 0x6A8EF2E9,
            SolarisHome = 0x6A90BA39,
            SolarisAlternate = 0x6A9283A5,
            SolarisReserved1 = 0x6A945A3B,
            SolarisReserved2 = 0x6A9630D1,
            SolarisReserved3 = 0x6A980767,
            SolarisReserved4 = 0x6A96237F,
            SolarisReserved5 = 0x6A8D2AC7,
            NetbsdSwap = 0x49F48D32,
            NetbsdFfs = 0x49F48D5A,
            NetbsdLfs = 0x49F48D82,
            NetbsdRaid = 0x49F48DAA,
            NetbsdConcat = 0x2DB519C4,
            NetbsdEncrypt = 0x2DB519EC,
            ChromeosKernel = 0xFE3A2A5D,
            ChromeosRootfs = 0x3CB8E202,
            ChromeosFuture = 0x2E0A753D,
            Haiku = 0x42465331,
            MidnightbsdBoot = 0x85D5E45E,
            MidnightbsdData = 0x85D5E45A,
            MidnightbsdSwap = 0x85D5E45B,
            MidnightbsdUfs = 0x0394EF8B,
            MidnightbsdVinum = 0x85D5E45C,
            MidnightbsdZfs = 0x85D5E45D,
            CephJournal = 0x45B0969E,
            CephEncrypt = 0x45B0969E,
            CephOsd = 0x4FBD7E29,
            CephEncryptOsd = 0x4FBD7E29,
            CephCreate = 0x89C57F98,
            CephEncryptCreate = 0x89C57F98,
            Openbsd = 0x824CC7A0,
            Qnx = 0xCEF5A9AD,
            Plan9 = 0xC91818F9,
            VmwareVmkcore = 0x9D275380,
            VmwareVmfs = 0xAA31E02A,
            VmwareReserved = 0x9198EFFC,
        }

        [Flags]
        public enum GuidFlags : ulong
        {
            System = 1 << 0,
            Ignore = 1 << 1,
            Legacy = 1 << 2,
            ReadOnly = 1 << 3,
            Hidden = 1 << 4,
            NoMount = 1 << 5
        }

        public IEnumerator<EfiPartitionEntry> GetEnumerator()
        {
            return Table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
