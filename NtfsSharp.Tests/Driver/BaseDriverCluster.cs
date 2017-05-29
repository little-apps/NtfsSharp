using System;
using System.Runtime.InteropServices;

namespace NtfsSharp.Tests.Driver
{
    public abstract class BaseDriverCluster : Byteable
    {
        public BaseDriverCluster()
        {
        }
        
        protected abstract bool ShouldGenerateDefault { get; }

        /// <summary>
        /// Generates the default data for the part
        /// </summary>
        protected abstract void GenerateDefaultDummy();

        /// <summary>
        /// Builds the bytes for the cluster in the NTFS volume
        /// </summary>
        /// <returns>Part in bytes</returns>
        /// <remarks>The size of the part should be no greater than 4096 bytes</remarks>
        public abstract byte[] Build();

        /// <summary>
        /// Reads the data as a cluster (8 sectors)
        /// </summary>
        /// <returns>The bytes in the cluster</returns>
        /// <remarks>The part will be resized to the size of cluster (4096 bytes) if it's not already</remarks>
        public byte[] ReadAsCluster()
        {
            if (ShouldGenerateDefault)
                GenerateDefaultDummy();

            var partBytes = Build();

            if (partBytes.Length == DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster)
                return partBytes;

            var clusterBytes = new byte[DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster];

            Array.Copy(partBytes, 0, clusterBytes, 0, partBytes.Length);

            return clusterBytes;
        }
    }
}
