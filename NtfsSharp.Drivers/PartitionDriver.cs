﻿using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace NtfsSharp.Drivers
{
    public class PartitionDriver : BaseDiskDriver
    {
        private SafeFileHandle Handle { get; }

        public readonly string Path;

        public PartitionDriver(string path)
        {
            Path = path;
            Handle = CreateFile(path, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            if (Handle.IsClosed || Handle.IsInvalid)
                throw new Win32Exception(Marshal.GetHRForLastWin32Error());
        }

        public override long Move(long offset, MoveMethod moveMethod = MoveMethod.Begin)
        {
            long newOffset = 0;

            if (!SetFilePointerEx(Handle, offset, out newOffset, moveMethod))
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

            Array.Resize(ref buffer, (int) bytesToRead);

            return buffer;
        }

        public override byte[] ReadFile(uint bytesToRead)
        {
            var buffer = new byte[bytesToRead];
            uint bytesRead;

            if (!ReadFile(Handle, buffer, bytesToRead, out bytesRead, IntPtr.Zero))
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