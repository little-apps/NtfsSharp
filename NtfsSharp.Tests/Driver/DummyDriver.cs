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
        public Dictionary<long, BaseDriverCluster> Clusters { get; } = new Dictionary<long, BaseDriverCluster>();

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
            var bytes = new byte[bytesToRead];
            var bytesIndex = (long) 0;
            var bytesRemaining = (long) bytesToRead;

            while (bytesRemaining > 0)
            {
                var lcn = _currentOffset / (BytesPerSector * SectorsPerCluster);
                var offsetInLcn = _currentOffset % (BytesPerSector * SectorsPerCluster);
                var bytesRemainingInLcn = BytesPerSector * SectorsPerCluster - offsetInLcn;

                // If cluster doesn't exist, don't stop here. Add blank byte array and continue on to next cluster.
                var clusterData = !Clusters.ContainsKey(lcn) ? new byte[BytesPerSector * SectorsPerCluster] : Clusters[lcn].ReadAsCluster();

                Array.Copy(clusterData, offsetInLcn, bytes, bytesIndex, bytesRemainingInLcn > bytesRemaining ? bytesRemaining : bytesRemainingInLcn);

                bytesIndex += bytesRemainingInLcn;
                _currentOffset += bytesRemainingInLcn;
                bytesRemaining -= bytesRemainingInLcn;
            }

            bytesRead = (uint) bytesIndex;
            return bytes;
        }

        public override void Dispose()
        {
           
        }
    }
}
