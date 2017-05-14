using System;
using System.Runtime.InteropServices;

namespace NtfsSharp.Data
{
    public class Sector
    {
        private ushort BytesPerSector => (ushort) (_volume != null ? _volume.BytesPerSector : 512);

        private byte[] _data;
        private readonly Volume _volume;

        public readonly ulong Offset;

        public byte[] Data
        {
            get
            {
                if (_data != null)
                    return _data;

                _volume.Driver.Move((long) Offset);
                _data = _volume.Driver.ReadFile(BytesPerSector);

                return _data;
            }
        }

        public Sector(ulong offset, Volume vol)
        {
            if (ReferenceEquals(null, vol))
                throw new ArgumentNullException(nameof(vol), "Volume cannot be null.");

            Offset = offset;
            _volume = vol;
        }

        public Sector(ulong offset, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null");

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
