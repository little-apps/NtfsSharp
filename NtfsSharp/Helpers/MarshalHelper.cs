using System;
using System.Runtime.InteropServices;

namespace NtfsSharp.Helpers
{
    internal static class MarshalHelper
    {
        internal static T ToStructure<T>(this byte[] bytes, uint offset = 0)
        {
            var bytesPtr = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            var ret = Marshal.PtrToStructure<T>(IntPtr.Add(bytesPtr.AddrOfPinnedObject(), (int) offset));

            bytesPtr.Free();

            return ret;
        }

        internal static byte[] GetBytesAtOffset(this byte[] bytes, uint offset, uint length)
        {
            var newBytes = new byte[length];

            Array.Copy(bytes, (int)offset, newBytes, 0, newBytes.Length);

            return newBytes;
        }
    }
}
