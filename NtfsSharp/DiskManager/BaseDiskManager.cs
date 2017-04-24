using System;
using System.Threading;

namespace NtfsSharp.DiskManager
{
    public abstract class BaseDiskManager : IDisposable
    {
        public abstract long Move(ulong offset, MoveMethod moveMethod = MoveMethod.Begin);
        public abstract byte[] ReadFile(uint bytesToRead);
        public abstract byte[] ReadFile(uint bytesToRead, out uint bytesRead);
        public abstract byte[] ReadFile(uint bytesToRead, out uint bytesRead, ref NativeOverlapped overlapped);
        public abstract byte[] SafeReadFile(uint bytesToRead);

        public enum MoveMethod : uint
        {
            Begin = 0,
            Current = 1,
            End = 2
        }

        public abstract void Dispose();
    }
}