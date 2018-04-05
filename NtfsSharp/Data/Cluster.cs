using System;
using System.Runtime.InteropServices;

namespace NtfsSharp.Data
{
    public class Cluster : IComparable, IComparable<Cluster>
    {
        private byte[] _data;
        private Sector[] _sectors;

        private readonly Volume _volume;

        public readonly ulong Lcn;

        /// <summary>
        /// The data contained in entire cluster. The number of bytes is <seealso cref="Volume.BytesPerSector"/> * <seealso cref="Volume.SectorsPerCluster"/>.
        /// </summary>
        /// <remarks>The data is stored once (and only once) this property is accessed</remarks>
        public byte[] Data
        {
            get
            {
                if (_data != null)
                    return _data;

                _data = DataOnDemand();

                return _data;
            }
        }

        /// <summary>
        /// <seealso cref="Sector"/> objects contained in cluster. The number of sectors is <seealso cref="Volume.SectorsPerCluster"/>
        /// </summary>
        /// <remarks>The sectors are stored once (and only once) this property is accessed</remarks>
        public Sector[] Sectors
        {
            get
            {
                if (_sectors != null)
                    return _sectors;

                var data = _data ?? DataOnDemand();
                _sectors = new Sector[_volume.SectorsPerCluster];

                for (var i = 0; i < _volume.SectorsPerCluster; i++)
                {
                    var sectorData = new byte[_volume.BytesPerSector];
                    Array.Copy(data, i * _volume.BytesPerSector, sectorData, 0, _volume.BytesPerSector);

                    Sectors[i] =
                        new Sector(
                            (Lcn * _volume.BytesPerSector * _volume.SectorsPerCluster) +
                            (ulong) (i * _volume.BytesPerSector), sectorData);
                }

                return _sectors;
            }
        }

        /// <summary>
        /// Constructor for Cluster
        /// </summary>
        /// <param name="lcn">Logical cluster number on <seealso cref="Volume"/></param>
        /// <param name="vol">Volume containg cluster</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="vol"/> is null.</exception>
        public Cluster(ulong lcn, Volume vol)
        {
            if (ReferenceEquals(null, vol))
                throw new ArgumentNullException(nameof(vol), "Volume cannot be null.");

            _volume = vol;
            Lcn = lcn;
        }

        /// <summary>
        /// Reads the data on demand from the volume
        /// </summary>
        /// <returns>Byte array that is <seealso cref="Volume.BytesPerSector"/> * <seealso cref="Volume.SectorsPerCluster"/> in length</returns>
        private byte[] DataOnDemand()
        {
            _volume.Driver.Move((long) (Lcn * _volume.BytesPerSector * _volume.SectorsPerCluster));
            return _volume.Driver.ReadSectorBytes(_volume.BytesPerSector * _volume.SectorsPerCluster);
        }

        /// <summary>
        /// Reads data as specified type
        /// </summary>
        /// <typeparam name="T">Type to read as</typeparam>
        /// <param name="offset">Offset on cluster</param>
        /// <returns>Instance of <typeparamref name="T"/> using data</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> + the size of <typeparamref name="T"/> is greater than the cluster size.</exception>
        public T ReadFile<T>(uint offset)
        {
            var bytesToRead = Marshal.SizeOf<T>();

            if (offset + bytesToRead > Data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            var gcHandle = GCHandle.Alloc(Data, GCHandleType.Pinned);

            var ret = Marshal.PtrToStructure<T>(IntPtr.Add(gcHandle.AddrOfPinnedObject(), (int) offset));

            gcHandle.Free();

            return ret;
        }

        public int CompareTo(object obj)
        {
            var other = obj as Cluster;

            if (other == null)
                return -1;

            return CompareTo(other);
        }

        public int CompareTo(Cluster other)
        {
            if (other == null)
                return -1;

            if (_volume != other._volume)
                return -1;

            return (int) (Lcn - other.Lcn);
        }
    }
}
