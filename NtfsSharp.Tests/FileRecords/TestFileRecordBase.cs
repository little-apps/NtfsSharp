using NtfsSharp.Facades;
using NtfsSharp.Files;
using NtfsSharp.Tests.Driver;
using NUnit.Framework;

namespace NtfsSharp.Tests.FileRecords
{
    [TestFixture]
    public abstract class TestFileRecordBase
    {
        public const uint MftRecordNum = 5;
        public uint BytesPerFileRecord => (uint)1 << 256 - BootSector.DummyBootSector.ClustersPerMFTRecord;

        internal DummyDriver Driver { get; set; }
        internal Volume Volume { get; set; }
        internal BootSector BootSector { get; set; }
        internal DummyFileRecord DummyFileRecord { get; set; }

        [SetUp]
        public void SetUpDummyDisk()
        {
            Driver = new DummyDriver();
            Volume = new Volume(Driver);
            BootSector = new BootSector();

            Driver.Clusters.Add(0, BootSector);

            Volume.ReadBootSector();

            DummyFileRecord = DummyFileRecord.BuildDummyFileRecord(MftRecordNum);
        }

        [TearDown]
        public void DisposeDummyDisk()
        {
            Volume.Dispose();
        }

        /// <summary>
        /// Reads the <see cref="Tests.Driver.DummyFileRecord"/> and gets the actual <see cref="FileRecord"/> from it
        /// </summary>
        /// <param name="readAttributes">If true, reads attributes once opened. (default: true)</param>
        /// <returns>FileRecord</returns>
        protected FileRecord ReadDummyFileRecord(bool readAttributes = true)
        {
            var dummyFileRecord = DummyFileRecord.BuildWithUsa(BytesPerFileRecord, Driver, 0xab);
            var fileRecord = readAttributes
                ? FileRecordAttributesFacade.Build(dummyFileRecord, Volume)
                : FileRecordFacade.Build(dummyFileRecord, Volume);

            return fileRecord;
        }
    }
}
