using System;
using System.IO;
using System.Runtime.InteropServices;
using NtfsSharp.Drivers.Physical;
using NtfsSharp.Drivers.Vhd.Data;

namespace NtfsSharp.Drivers
{
    public class VhdDriver : BaseDiskDriver
    {
        private long _currentOffset = 0;
        private readonly ulong _maxSizeBytes;

        private readonly Vhd.Vhd _vhd;
        private readonly MasterBootRecord _masterBootRecord;
        private readonly Partition _currentPartition;

        private uint CurrentSectorOffset
        {
            get { return (uint) (_currentOffset / Sector.BytesPerSector); }
        }

        public VhdDriver(string filePath, uint partition = 0) : this(new FileStream(filePath, FileMode.Open), partition)
        {
            
        }

        public VhdDriver(Stream stream, uint partition = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException("Must be able to read from stream.", nameof(stream));

            _vhd = new Vhd.Vhd(stream);

            _maxSizeBytes = _vhd.Image.TotalSectors * Sector.BytesPerSector;

            _masterBootRecord = new MasterBootRecord(this);
            _currentPartition = _masterBootRecord.SelectPartition(partition);
        }

        public override long Move(long offset, MoveMethod moveMethod = MoveMethod.Begin)
        {
            var startOffset = _currentPartition?.StartSector * 512 ?? 0;
            var endOffset = _currentPartition?.EndSector * 512 ?? _maxSizeBytes;

            switch (moveMethod)
            {
                case MoveMethod.Begin:
                    _currentOffset = (long) (startOffset + (ulong) offset);
                    break;
                case MoveMethod.Current:
                    _currentOffset += offset;
                    break;
                case MoveMethod.End:
                    _currentOffset = (long) (endOffset + (ulong) offset);
                    break;
            }

            return _currentOffset;
        }

        public override byte[] ReadSectorBytes(uint bytesToRead)
        {
            var dest = new byte[bytesToRead];
            uint currentOffset = 0;

            uint bytesRead = 0;

            while (bytesRead < bytesToRead)
            {
                var sector = _vhd.ReadSector(CurrentSectorOffset);

                Array.Copy(sector.Data, 0, dest, currentOffset, sector.Data.Length);

                currentOffset += (uint)sector.Data.Length;
                bytesRead += (uint)sector.Data.Length;
                _currentOffset += sector.Data.Length;
            }

            return dest;
        }

        public override byte[] ReadInsideSectorBytes(uint bytesToRead)
        {
            var startSectorOffset = _currentOffset % Sector.BytesPerSector;

            var startSector = CurrentSectorOffset;
            // Subtract the start sector offset from bytes to read, divide by the bytes per sector (512) and add 1 (for the first sector that was subtracted)
            var totalSectors = ((bytesToRead - startSectorOffset) / Sector.BytesPerSector) + 1;

            var currentOffset = 0;
            var data = new byte[bytesToRead];

            for (var i = CurrentSectorOffset; i < totalSectors; i++)
            {
                var sector = _vhd.ReadSector(i);

                var sectorDataToRead = 512 - (_currentOffset % Sector.BytesPerSector);

                Array.Copy(sector.Data, i == startSector ? startSectorOffset : 0, data, currentOffset, sectorDataToRead);
            }

            return data;
        }

        public override void Dispose()
        {
            _vhd.Dispose();
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
