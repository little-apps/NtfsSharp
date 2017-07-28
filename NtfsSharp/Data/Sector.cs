using System;
using System.Runtime.InteropServices;

namespace NtfsSharp.Data
{
    public class Sector
    {
        /// <summary>
        /// Specifys the BytesPerSector. The value from <seealso cref="Volume"/> is used if it is not null, otherwise, 512 is used.
        /// </summary>
        private ushort BytesPerSector => (ushort) (_volume != null ? _volume.BytesPerSector : 512);

        private byte[] _data;
        private readonly Volume _volume;

        public readonly ulong Offset;

        /// <summary>
        /// Data contained in sector
        /// </summary>
        /// <remarks>If using Volume, data is retreived once (and only once) when this property is accessed.</remarks>
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

        /// <summary>
        /// Constructor for Sector on <seealso cref="Volume"/>
        /// </summary>
        /// <param name="offset">Offset of sector</param>
        /// <param name="vol">Volume containing sector</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="vol"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> is not a multiple of <seealso cref="Volume.BytesPerSector"/></exception>
        public Sector(ulong offset, Volume vol)
        {
            if (ReferenceEquals(null, vol))
                throw new ArgumentNullException(nameof(vol), "Volume cannot be null.");

            if (offset % vol.BytesPerSector > 0)
                throw new ArgumentOutOfRangeException(nameof(offset),
                    $"Offset must be multiple of {vol.BytesPerSector}");

            Offset = offset;
            _volume = vol;
        }

        /// <summary>
        /// Constructor for Sector with data for it specified
        /// </summary>
        /// <param name="offset">Offset that sector is located</param>
        /// <param name="data">Data contained in sector</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="data"/> is not <seealso cref="BytesPerSector"/></exception>
        public Sector(ulong offset, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null");

            if (data.Length != BytesPerSector)
                throw new ArgumentOutOfRangeException(nameof(data), $"Data must be {BytesPerSector} bytes.");

            Offset = offset;
            _data = data;
        }

        /// <summary>
        /// Reads data in sector as specified type
        /// </summary>
        /// <typeparam name="T">Type to return from data</typeparam>
        /// <param name="offset">Offset in sector to read from</param>
        /// <returns>Instance of <typeparamref name="T"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> + the size of <typeparamref name="T"/> is greater than <seealso cref="BytesPerSector"/></exception>
        public T ReadFile<T>(uint offset)
        {
            var bytesToRead = Marshal.SizeOf<T>();

            if (offset + bytesToRead > BytesPerSector)
                throw new ArgumentOutOfRangeException(nameof(offset));

            var gcHandle = GCHandle.Alloc(Data, GCHandleType.Pinned);

            var ret = Marshal.PtrToStructure<T>(IntPtr.Add(gcHandle.AddrOfPinnedObject(), (int) offset));

            gcHandle.Free();

            return ret;
        }
    }
}
