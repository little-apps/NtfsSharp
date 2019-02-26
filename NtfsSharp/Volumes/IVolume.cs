using System;
using System.Collections.Generic;
using NtfsSharp.Data;
using NtfsSharp.FileRecords;

namespace NtfsSharp.Volumes
{
    public interface IVolume : IComparable<IVolume>
    {
        IDiskDriver Driver { get; }

        ulong MftLcn { get; }

        uint SectorsPerCluster { get; }
        ushort BytesPerSector { get; }
        uint SectorsPerMftRecord { get; }

        IReadOnlyDictionary<uint, FileRecord> MFT { get; }

        void Read();
        Cluster ReadLcn(ulong lcn);
    }
}
