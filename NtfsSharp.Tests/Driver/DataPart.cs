using System;

namespace NtfsSharp.Tests.Driver
{
    class DataPart : BaseDriverPart
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

        public override byte[] BuildPart()
        {
            return Data;
        }
    }
}
