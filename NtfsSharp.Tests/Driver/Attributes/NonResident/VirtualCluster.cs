using System;

namespace NtfsSharp.Tests.Driver.Attributes.NonResident
{
    public class VirtualCluster : BaseDriverCluster
    {
        public readonly uint Vcn;
        public readonly byte[] Bytes;

        protected override bool ShouldGenerateDefault { get; } = false;

        public VirtualCluster(uint vcn, byte[] bytes = null)
        {
            if (bytes == null)
                bytes = new byte[DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster];

            if (bytes.Length > DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster)
                throw new ArgumentOutOfRangeException(nameof(bytes),
                    $"Bytes cannot be larger than {DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster} bytes");

            Vcn = vcn;
            Bytes = bytes;
        }

        protected override void GenerateDefaultDummy()
        {
            throw new System.NotImplementedException();
        }

        public override byte[] Build()
        {
            return Bytes;
        }
    }
}
