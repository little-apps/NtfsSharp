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

                    Sectors[i] = new Sector((Lcn * _volume.BytesPerSector * _volume.SectorsPerCluster) + (ulong)(i * _volume.BytesPerSector), sectorData);
                }

                return _sectors;
            }
        }

        public Cluster(ulong lcn, Volume vol)
        {
            _volume = vol;
            Lcn = lcn;
        }

        private byte[] DataOnDemand()
        {
            _volume.Driver.Move((long)(Lcn * _volume.BytesPerSector * _volume.SectorsPerCluster));
            return _volume.Driver.ReadFile(_volume.BytesPerSector * _volume.SectorsPerCluster);
        }

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
