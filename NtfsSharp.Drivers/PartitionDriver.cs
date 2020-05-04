using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace NtfsSharp.Drivers
{
    public class PartitionDriver : BaseDiskDriver
    {
        private FileStream FileStream { get; }

        public readonly string Path;

        public PartitionDriver(string path)
        {
            Path = path;

                var fileHandle = CreateFile(path, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

                if (fileHandle.IsClosed || fileHandle.IsInvalid)
                    throw new Win32Exception(Marshal.GetHRForLastWin32Error());

                FileStream = new FileStream(fileHandle, FileAccess.Read, 4096, false);
        }

        public override long Move(long offset, MoveMethod moveMethod = MoveMethod.Begin)
        {
            SeekOrigin seekOrigin;
            switch (moveMethod)
            {
                case MoveMethod.Current:
                    seekOrigin = SeekOrigin.Current;
                    break;
                    

                case MoveMethod.End:
                    seekOrigin = SeekOrigin.End;
                    break;

                default:
                    seekOrigin = SeekOrigin.Begin;
                    break;
            }

            return FileStream.Seek(offset, seekOrigin);
        }

        private static byte[] AllocateByteArray(uint bytesToRead, out uint leftOverBytes)
        {
            leftOverBytes = 512 - bytesToRead % 512;

            return new byte[bytesToRead + leftOverBytes];
        }

        public override byte[] ReadInsideSectorBytes(uint bytesToRead)
        {
            var buffer = AllocateByteArray(bytesToRead, out _);

            FileStream.Read(buffer, 0, buffer.Length);

            Array.Resize(ref buffer, (int) bytesToRead);

            return buffer;
        }

        public override byte[] ReadSectorBytes(uint bytesToRead)
        {
            var buffer = new byte[bytesToRead];

            FileStream.Read(buffer, 0, (int) bytesToRead);

            return buffer;
        }

        public override void Dispose()
        {
            FileStream.Close();
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove,
            [Out] out long lpNewFilePointer, MoveMethod dwMoveMethod);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(SafeFileHandle hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead, [In, Out] ref NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(SafeFileHandle hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(SafeFileHandle hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead, IntPtr lpOverlapped);
    }
}
