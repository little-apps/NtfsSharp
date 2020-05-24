using System;
using System.Collections.Generic;
using NtfsSharp.FileRecords;
using NtfsSharp.Units;

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

        IVolume Read();
        Cluster ReadLcn(ulong lcn);
    }
}
