using System;
using System.IO;
using System.Runtime.InteropServices;
using NtfsSharp.Drivers.Vhd.Data;
using NtfsSharp.Drivers.Vhd.ImageTypes;
using NtfsSharp.Helpers;

namespace NtfsSharp.Drivers.Vhd
{
    public class Vhd : IDisposable
    {
        public Stream Stream { get; }

        public VhdFooter Footer { get; private set; }

        public BaseImage Image { get; private set; }

        public Vhd(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException(nameof(stream));

            Stream = stream;

            ReadFooter();
            ReadImage();
        }

        private void ReadFooter()
        {
            Stream.Seek(-1 * Marshal.SizeOf<VhdFooter>(), SeekOrigin.End);

            var bytes = new byte[Marshal.SizeOf<VhdFooter>()];
            Stream.Read(bytes, 0, bytes.Length);

            Footer = bytes.ToStructure<VhdFooter>(MarshalHelper.Endianness.BigEndian);
        }

        private void ReadImage()
        {
            switch (Footer.DiskType)
            {
                case DiskTypes.Fixed:
                    throw new NotImplementedException();
                case DiskTypes.Dynamic:
                    Image = new DynamicImage(this);
                    break;
            }
        }

        public Sector ReadSector(uint sector)
        {
            return Image.ReadSector(sector);
        }

        public void Dispose()
        {
            Stream?.Dispose();
        }

        [Flags]
        public enum VhdFeatures : uint
        {
            None = 0,
            Temporary = 1,
            Reserved = 2
        }

        public enum HostOses : uint
        {
            Windows = 0x5769326B, // "Wi2k"
            Macintosh = 0x4D616320 // "Mac "
        }

        public enum DiskTypes : uint
        {
            None = 0,
            Reserved1 = 1,
            Fixed = 2,
            Dynamic = 3,
            Differencing = 4,
            Reserved2 = 5,
            Reserved3 = 6
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct VhdFooter
        {
            public long Cookie;
            public VhdFeatures Features;
            public ushort FileFormatVersionLow;
            public ushort FileFormatVersionHigh;
            public ulong DataOffset;
            public uint Timestamp;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string CreatorApplication;
            public ushort CreatorVersionLow;
            public ushort CreatorVersionHigh;
            public HostOses CreatorHostOs;
            public ulong OriginalSize;
            public ulong CurrentSize;
            public ushort Cylinder;
            public byte Heads;
            public byte SectorsPerTrack;
            public DiskTypes DiskType;
            public uint Checksum;
            public Guid UniqueId;
            [MarshalAs(UnmanagedType.I1)]
            public bool SavedState;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 427)]
            public byte[] Reserved;
        }
    }
}
