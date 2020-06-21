using System;
using NtfsSharp.Units;

namespace NtfsSharp.Volumes
{
    public interface IVolume : IComparable<IVolume>
    {
        /// <summary>
        /// Number of sectors per cluster on the volume.
        /// </summary>
        uint SectorsPerCluster { get; }

        /// <summary>
        /// Number of bytes per sector on the volume.
        /// </summary>
        ushort BytesPerSector { get; }

        /// <summary>
        /// Reads the structure of the volume.
        /// </summary>
        /// <returns>Instance of current <see cref="IVolume"/>.</returns>
        IVolume Read();

        /// <summary>
        /// Creates an object representing a cluster at logical cluster number (LCN) on the volume.
        /// </summary>
        /// <param name="lcn">Logical Cluster Number</param>
        /// <returns>Instance of <seealso cref="Cluster"/></returns>
        Cluster ReadCluster(ulong lcn);

        /// <summary>
        /// Creates an object representing a sector at index on the volume.
        /// </summary>
        /// <param name="index">Index of sector.</param>
        /// <returns>Instance of <seealso cref="Sector"/></returns>
        Sector ReadSector(ulong index);
    }
}
