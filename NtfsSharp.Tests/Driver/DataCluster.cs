using System;

namespace NtfsSharp.Tests.Driver
{
    class DataCluster : BaseDriverCluster
    {
        public byte[] Data { get; } = new byte[DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster];

        protected override bool ShouldGenerateDefault
        {
            get { return false; }
        }

        protected override void GenerateDefaultDummy()
        {
            throw new NotImplementedException();
        }

        public override byte[] Build()
        {
            return Data;
        }
    }
}
