using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using NtfsSharp.Drivers.Physical.Exceptions;
using NtfsSharp.Helpers;

namespace NtfsSharp.Drivers.Physical
{
    public class PhysicalDiskDriver : BaseDiskDriver
    {
        private SafeFileHandle Handle { get; }
        private MasterBootRecord Mbr { get; }
        private Partition SelectedPartition { get; }

        /// <summary>
        /// Constructor for physical disk manager
        /// </summary>
        /// <param name="physicalDrivePath">Path to physical drive (with trailing "\\.\")</param>
        /// <param name="partition">Partition number on physical drive (default: 0)</param>
        public PhysicalDiskDriver(string physicalDrivePath, uint partition = 0)
        {
            Handle = CreateFile(physicalDrivePath, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0,
                IntPtr.Zero);

            if (Handle.IsClosed || Handle.IsInvalid)
                throw new Win32Exception(Marshal.GetHRForLastWin32Error());

            Mbr = new MasterBootRecord(this);
            SelectedPartition = Mbr.SelectPartition(partition);
        }
        
        public override long Move(long offset, MoveMethod moveMethod = MoveMethod.Begin)
        {
            if (SelectedPartition != null)
            {
                switch (moveMethod)
                {
                    case MoveMethod.Begin:
                        offset = (long) (SelectedPartition.StartSector * 512 + (ulong) offset);
                        break;
                    case MoveMethod.End:
                        offset = offset + (long) (SelectedPartition.EndSector * 512);
                        moveMethod = MoveMethod.End;
                        break;
                }
            }

            if (!SetFilePointerEx(Handle, offset, out long newOffset, moveMethod))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return newOffset;
        }
        
        private static byte[] AllocateByteArray(uint bytesToRead, out uint leftOverBytes)
        {
            leftOverBytes = 512 - bytesToRead % 512;

            return new byte[bytesToRead + leftOverBytes];
        }

        public override byte[] SafeReadFile(uint bytesToRead)
        {
            var buffer = AllocateByteArray(bytesToRead, out uint leftOverBytes);

            if (!ReadFile(Handle, buffer, (uint)buffer.Length, out uint bytesRead, IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            Array.Resize(ref buffer, (int)bytesToRead);

            return buffer;
        }

        public override byte[] ReadFile(uint bytesToRead)
        {
            var buffer = new byte[bytesToRead];

            if (!ReadFile(Handle, buffer, bytesToRead, out uint bytesRead, IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return buffer;
        }

        public override byte[] ReadFile(uint bytesToRead, out uint bytesRead)
        {
            var buffer = new byte[bytesToRead];

            if (!ReadFile(Handle, buffer, bytesToRead, out bytesRead, IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return buffer;
        }

        public override byte[] ReadFile(uint bytesToRead, out uint bytesRead, ref NativeOverlapped overlapped)
        {
            var buffer = new byte[bytesToRead];

            if (!ReadFile(Handle, buffer, bytesToRead, out bytesRead, ref overlapped))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return buffer;
        }

        public override void Dispose()
        {
            Handle?.Dispose();
        }

        public static IEnumerable<string> GetPhysicalDrives()
        {
            const int errorInsufficientBuffer = 0x7A;

            uint bufferSize = 128;
            var bufferPtr = Marshal.AllocHGlobal((int) bufferSize);

            while (QueryDosDevice(null, bufferPtr, bufferSize) == 0)
            {
                if (Marshal.GetLastWin32Error() != errorInsufficientBuffer)
                {
                    Marshal.FreeHGlobal(bufferPtr);

                    throw Marshal.GetExceptionForHR(Marshal.GetLastWin32Error());
                }

                bufferSize *= 5;

                bufferPtr = Marshal.ReAllocHGlobal(bufferPtr, (IntPtr) bufferSize);
            }

            var buffer = Marshal.PtrToStringAnsi(bufferPtr, (int) bufferSize);

            Marshal.FreeHGlobal(bufferPtr);

            return
                buffer.Substring(0, buffer.IndexOf("\0\0", StringComparison.Ordinal))
                    .Split('\0')
                    .Where(device => device.StartsWith("PhysicalDrive"));
        }

        

        public enum BootIndicator : byte
        {
            NoBoot = 0,
            SystemPartition = 128
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint QueryDosDevice(string lpDeviceName, IntPtr lpTargetPath, uint ucchMax);

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
    }
}
