using NtfsSharp.Drivers;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NtfsSharp.Tests.Driver
{
    internal class DummyDriver : BaseDiskDriver
    {
        public const uint DriveSize = 5 * 1024 * 1024;
        public const uint BytesPerSector = 512;
        public const uint SectorsPerCluster = 8;

        public const uint MasterFileTableLcn = 1;
        
        /// <summary>
        /// The parts of the NTFS Volume.
        /// </summary>
        /// <remarks>Each key represents the LCN on the volume.</remarks>
        public Dictionary<long, BaseDriverPart> Parts { get; } = new Dictionary<long, BaseDriverPart>();

        private long _currentOffset;

        public DummyDriver()
        {

        }

        public override long Move(long offset, MoveMethod moveMethod = MoveMethod.Begin)
        {
            long newOffset;

            switch (moveMethod)
            {
                case MoveMethod.Begin:
                    newOffset = offset;
                    break;
                case MoveMethod.Current:
                case MoveMethod.End:
                    newOffset = _currentOffset + offset;
                    break;
                default:
                    throw new ArgumentException("MoveMethod is not valid", nameof(moveMethod));
            }

            if (newOffset > DriveSize)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset is past end of disk");

            _currentOffset = newOffset;

            return _currentOffset;
        }

        public override byte[] ReadFile(uint bytesToRead)
        {
            if (bytesToRead % 512 != 0)
                throw new ArgumentException("Bytes to read must be multiple of 512", nameof(bytesToRead));

            return InternalReadFile(bytesToRead, out uint _);
        }

        public override byte[] ReadFile(uint bytesToRead, out uint bytesRead)
        {
            if (bytesToRead % 512 != 0)
                throw new ArgumentException("Bytes to read must be multiple of 512", nameof(bytesToRead));

            return InternalReadFile(bytesToRead, out bytesRead);
        }

        public override byte[] ReadFile(uint bytesToRead, out uint bytesRead, ref NativeOverlapped overlapped)
        {
            if (bytesToRead % 512 != 0)
                throw new ArgumentException("Bytes to read must be multiple of 512", nameof(bytesToRead));

            var newOffset = Move((overlapped.OffsetHigh << 32) + overlapped.OffsetLow);

            overlapped.OffsetHigh = (int) (newOffset >> 32);
            overlapped.OffsetLow = (int) (newOffset & 0xFFFFFFFF);

            return InternalReadFile(bytesToRead, out bytesRead);
        }

        public override byte[] SafeReadFile(uint bytesToRead)
        {
            var buffer = InternalReadFile(bytesToRead, out uint bytesRead);

            Array.Resize(ref buffer, (int)bytesToRead);

            return buffer;
        }

        private byte[] InternalReadFile(uint bytesToRead, out uint bytesRead)
        {
            var lcn = _currentOffset / (BytesPerSector * SectorsPerCluster);
            var offsetInLcn = _currentOffset % (BytesPerSector * SectorsPerCluster);

            if (!Parts.ContainsKey(lcn))
            {
                bytesRead = (uint)(BytesPerSector * SectorsPerCluster - offsetInLcn);
                return new byte[bytesRead];
            }

            var clusterBytes = Parts[lcn].ReadAsCluster();

            if (offsetInLcn == 0)
            {
                bytesRead = (uint)clusterBytes.Length;
                return clusterBytes;
            }
            
            var retBytes = new byte[bytesToRead];

            Array.Copy(clusterBytes, (int)offsetInLcn, retBytes, 0, bytesToRead);

            bytesRead = (uint) retBytes.Length;

            return retBytes;
        }

        public override void Dispose()
        {
           
        }
    }
}
