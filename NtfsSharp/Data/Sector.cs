using System;
using System.Runtime.InteropServices;

namespace NtfsSharp.Data
{
    public class Sector
    {
        public static ushort BytesPerSector = 512;

        public readonly ulong Offset;
        public readonly byte[] Data;

        public Sector(ulong offset, Volume vol)
        {
            Offset = offset;

            vol.Disk.Move(offset);
            Data = vol.Disk.ReadFile(vol.BytesPerSector);
        }

        public Sector(ulong offset, byte[] data)
        {
            Offset = offset;
            Data = data;
        }

        public T ReadFile<T>(uint offset)
        {
            var bytesToRead = Marshal.SizeOf<T>();

            if (offset + bytesToRead > BytesPerSector)
                throw new ArgumentOutOfRangeException(nameof(offset));

            var gcHandle = GCHandle.Alloc(Data, GCHandleType.Pinned);

            var ret = Marshal.PtrToStructure<T>(IntPtr.Add(gcHandle.AddrOfPinnedObject(), (int)offset));

            gcHandle.Free();

            return ret;
        }
    }
}
