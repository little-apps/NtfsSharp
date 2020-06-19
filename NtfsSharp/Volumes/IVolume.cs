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

        /// <summary>
        /// Creates an object representing a sector at index on the volume.
        /// </summary>
        /// <param name="index">Index of sector.</param>
        /// <returns>Instance of <seealso cref="Sector"/></returns>
        Sector ReadSector(ulong index);
    }
}
