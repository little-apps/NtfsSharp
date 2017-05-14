﻿using System;
using System.Runtime.InteropServices;

namespace NtfsSharp.Data
{
    public class Sector
    {
        public static ushort BytesPerSector = 512;

        private byte[] _data;
        private readonly Volume Volume;

        public readonly ulong Offset;

        public byte[] Data
        {
            get
            {
                if (_data != null)
                    return _data;

                Volume.Driver.Move((long) Offset);
                _data = Volume.Driver.ReadFile(Volume.BytesPerSector);

                return _data;
            }
        }

        public Sector(ulong offset, Volume vol)
        {
            Offset = offset;
            Volume = vol;
        }

        public Sector(ulong offset, byte[] data)
        {
            Offset = offset;
            _data = data;
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
