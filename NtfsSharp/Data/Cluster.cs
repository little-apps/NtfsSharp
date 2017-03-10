using System;
using System.Runtime.InteropServices;

namespace NtfsSharp.Data
{
    public class Cluster : IComparable, IComparable<Cluster>
    {
        public readonly Sector[] Sectors;
        public readonly ulong Lcn;
        public readonly byte[] Data;

        public Cluster(ulong lcn, Volume vol)
        {
            Lcn = lcn;

            var offset = lcn * vol.BytesPerSector * vol.SectorsPerCluster;

            vol.Disk.Move(lcn * vol.BytesPerSector * vol.SectorsPerCluster);
            Data = vol.Disk.ReadFile(vol.BytesPerSector * vol.SectorsPerCluster);

            Sectors = new Sector[vol.SectorsPerCluster];

            for (var i = 0; i < vol.SectorsPerCluster; i++)
            {
                var sectorData = new byte[vol.BytesPerSector];
                Array.Copy(Data, i * vol.BytesPerSector, sectorData, 0, vol.BytesPerSector);

                Sectors[i] = new Sector(offset + (ulong) (i * vol.BytesPerSector), sectorData);
            }
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

            return (int) (Lcn - other.Lcn);
        }
    }
}
