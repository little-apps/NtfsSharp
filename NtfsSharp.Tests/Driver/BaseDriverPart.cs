using System;
using System.Runtime.InteropServices;

namespace NtfsSharp.Tests.Driver
{
    public abstract class BaseDriverPart
    {
        public BaseDriverPart()
        {
            GenerateDefaultDummy();
        }

        protected abstract void GenerateDefaultDummy();

        public abstract byte[] BuildPart();

        public byte[] ReadAsCluster()
        {
            var partBytes = BuildPart();

            if (partBytes.Length == DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster)
                return partBytes;

            var clusterBytes = new byte[DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster];

            Array.Copy(partBytes, 0, clusterBytes, 0, partBytes.Length);

            return clusterBytes;
        }

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
