using System;
using System.Runtime.InteropServices;

namespace NtfsSharp.Tests.Driver
{
    public abstract class BaseDriverCluster
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

        /// <summary>
        /// Converts a structure into it's bytes
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="structure">Structure to convert</param>
        /// <param name="size">Number of bytes to be return. If zero, the <seealso cref="Marshal"/>.SizeOf of the structure is used. (default: 0)</param>
        /// <returns>Byte array containing structure</returns>
        /// <remarks>If the size is bigger than the actual structure, any extra bytes will be 0x00.</remarks>
        protected byte[] StructureToBytes<T>(T structure, uint size = 0) where T:struct
        {
            if (size == 0)
                size = (uint) Marshal.SizeOf(structure);

            var arr = new byte[size];
            var ptr = Marshal.AllocHGlobal((int) size);

            Marshal.StructureToPtr(structure, ptr, true);

            Marshal.Copy(ptr, arr, 0, (int) size);

            Marshal.FreeHGlobal(ptr);

            return arr;
        }
    }
}
