using System;
using System.IO;
using System.Runtime.InteropServices;
using NtfsSharp.Drivers.Vhd.Data;
using NtfsSharp.Helpers;
using VhdParser.Data;

namespace NtfsSharp.Drivers.Vhd.ImageTypes
{
    public class DynamicImage : BaseImage
    {
        public ulong DiskSizeBytes => DynamicDiskHeader.BlockSize * DynamicDiskHeader.MaxTableEntries;

        public DynamicDiskHeaderStruct DynamicDiskHeader { get; private set; }

        public BlockAllocation BlockAllocationTable { get; private set; }

        public uint SectorsPerBlock => DynamicDiskHeader.BlockSize / Sector.BytesPerSector;

        public DynamicImage(Vhd vhd) : base(vhd)
        {
            ReadDynamicDiskHeader();
            ReadBlockAllocationTable();
        }

        private void ReadDynamicDiskHeader()
        {
            Vhd.Stream.Seek(512, SeekOrigin.Begin);

            var dynamicDiskHeaderBytes = new byte[1024];

            Vhd.Stream.Read(dynamicDiskHeaderBytes, 0, dynamicDiskHeaderBytes.Length);

            DynamicDiskHeader =
                dynamicDiskHeaderBytes.ToStructure<DynamicDiskHeaderStruct>(MarshalHelper.Endianness.BigEndian);

            // Each datablock can contain up to 512 * 8 sectors and the max datablocks is the max table entries
            TotalSectors = (512 * 8) * DynamicDiskHeader.MaxTableEntries;
        }

        private void ReadBlockAllocationTable()
        {
            var bytes = new byte[512];

            Vhd.Stream.Seek((long) DynamicDiskHeader.TableOffset, SeekOrigin.Begin);

            Vhd.Stream.Read(bytes, 0, bytes.Length);

            BlockAllocationTable = new BlockAllocation(bytes);
        }

        public override Sector ReadSector(uint sector)
        {
            // Determine which datablock sector is in
            var dataBlockIndex = sector / SectorsPerBlock;

            // Is datablock used?
            if (!BlockAllocationTable.Bitmap.Get((int) dataBlockIndex))
                return Sector.Null;

            var vhdFileLocation = BlockAllocationTable[dataBlockIndex] * Sector.BytesPerSector;

            // Get datablock
            var dataBlock = new DataBlock(vhdFileLocation, this);

            // Get sector # inside datablock
            var sectorInDataBlock = sector % SectorsPerBlock;

            return dataBlock.ReadSector(sectorInDataBlock);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DynamicDiskHeaderStruct
        {
            /// <summary>
            /// This field identifies the header. Should be set to "cxsparse" or 0x6378737061727365
            /// </summary>
            public ulong Cookie;
            /// <summary>
            /// This field contains the absolute byte offset to the next structure in the hard disk image. It is currently unused by existing formats and should be set to 0xFFFFFFFF.
            /// </summary>
            public ulong DataOffset;
            /// <summary>
            /// This field stores the absolute byte offset of the Block Allocation Table (BAT) in the file. 
            /// </summary>
            public ulong TableOffset;
            /// <summary>
            /// This field stores the version of the dynamic disk header. The field is divided into Major/Minor version. 
            /// The least-significant two bytes represent the minor version, and the most-significant two bytes represent the major version. 
            /// This must match with the file format specification. For this specification, this field must be initialized to 0x00010000. 
            /// The major version will be incremented only when the header format is modified in such a way that it is no longer compatible with older versions of the product.
            /// </summary>
            public uint HeaderVersion;
            /// <summary>
            /// This field holds the maximum entries present in the BAT. This should be equal to the number of blocks in the disk (that is, the disk size divided by the block size). 
            /// </summary>
            public uint MaxTableEntries;
            /// <summary>
            /// A block is a unit of expansion for dynamic and differencing hard disks. 
            /// It is stored in bytes. 
            /// This size does not include the size of the block bitmap.
            /// It is only the size of the data section of the block. 
            /// The sectors per block must always be a power of two. 
            /// The default value is 0x00200000 (indicating a block size of 2 MB).
            /// </summary>
            public uint BlockSize;
            /// <summary>
            /// This field holds a basic checksum of the dynamic header. It is a one’s complement of the sum of all the bytes in the header without the checksum field.
            /// If the checksum verification fails the file should be assumed to be corrupt.
            /// </summary>
            public uint Checksum;
            /// <summary>
            /// This field is used for differencing hard disks. A differencing hard disk stores a 128-bit UUID of the parent hard disk. 
            /// </summary>
            public Guid ParentGuid;
            /// <summary>
            /// This field stores the modification time stamp of the parent hard disk. This is the number of seconds since January 1, 2000 12:00:00 AM in UTC/GMT.
            /// </summary>
            public uint ParentTimestamp;
            public int Reserved1;
            /// <summary>
            /// This field contains a Unicode string (UTF-16) of the parent hard disk filename. 
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string ParentUnicodeNameBytes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public ParentLocatorTable[] ParentLocatorEntries;
            /// <summary>
            /// This must be initialized to zeroes.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] Reserved2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ParentLocatorTable
        {
            /// <summary>
            /// The platform code describes which platform-specific format is used for the file locator. 
            /// For Windows, a file locator is stored as a path (for example. “c:\disksimages\ParentDisk.vhd”). 
            /// On a Macintosh system, the file locator is a binary large object (blob) that contains an “alias.” 
            /// The parent locator table is used to support moving hard disk images across platforms.
            /// </summary>
            public PlatformCodes PlatformCode;
            /// <summary>
            /// This field stores the number of 512-byte sectors needed to store the parent hard disk locator.
            /// </summary>
            public uint PlatformDataSpace;
            /// <summary>
            /// This field stores the actual length of the parent hard disk locator in bytes.
            /// </summary>
            public uint PlatformDataLength;
            public uint Reserved;
            /// <summary>
            /// This field stores the absolute file offset in bytes where the platform specific file locator data is stored.
            /// </summary>
            public ulong PlatformDataOffset;
        }

        public enum PlatformCodes : uint
        {
            None = 0,
            Wi2r = 0x57693272,
            Wi2k = 0x5769326B,
            W2ru = 0x57327275,
            W2ku = 0x57326B75,
            Mac = 0x4D616320,
            MacX = 0x4D616358
        }
    }
}
