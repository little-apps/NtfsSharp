using NUnit.Framework;
using System;
using NtfsSharp.Exceptions;
using NtfsSharp.Tests.Driver;

namespace NtfsSharp.Tests
{
    [TestFixture]
    public class TestBootSector
    {
        public const ushort BytesPerSector = 512;
        public const byte SectorsPerCluster = 8;

        private DummyDriver Driver { get; set; }
        private Volume Volume { get; set; }
        private BootSector BootSector { get; set; }

        [SetUp]
        public void SetUpDummyDisk()
        {
            Driver = new DummyDriver();
            Volume = new Volume(Driver, false);
            BootSector = new BootSector();

            Driver.Parts[0] = BootSector;
        }

        [Test]
        public void TestClustersPerMFTRecordPositive()
        {
            const byte clustersPerMftRecord = 0x7F;

            BootSector.DummyBootSector.ClustersPerMFTRecord = clustersPerMftRecord;

            Volume.ReadBootSector();

            const int expected = clustersPerMftRecord * BytesPerSector * SectorsPerCluster;
            Assert.AreEqual(expected, Volume.BytesPerFileRecord);
            Assert.Greater(Volume.BytesPerFileRecord, 0);
        }

        [Test]
        public void TestClustersPerMFTRecordNegativeValid()
        {
            var clustersPerMftRecord = unchecked((sbyte) 0xF6);

            BootSector.DummyBootSector.ClustersPerMFTRecord = (byte) clustersPerMftRecord;

            Volume.ReadBootSector();

            var expected = (int) Math.Pow(2, Math.Abs(clustersPerMftRecord));
            Assert.AreEqual(expected, Volume.BytesPerFileRecord);
            Assert.Greater(Volume.BytesPerFileRecord, 0);
        }

        [Test]
        public void TestClustersPerMFTRecordNegativeInvalid()
        {
            var clustersPerMftRecord = unchecked((sbyte)0x80);

            BootSector.DummyBootSector.ClustersPerMFTRecord = (byte)clustersPerMftRecord;

            var ex = Assert.Throws<InvalidBootSectorException>(Volume.ReadBootSector);
            Assert.AreEqual(nameof(BootSector.DummyBootSector.ClustersPerMFTRecord), ex.FieldName);
        }

        [Test]
        public void TestBytesPerSectorValid()
        {
            const ushort bytesPerSector = 512;

            BootSector.DummyBootSector.BytesPerSector = bytesPerSector;

            Volume.ReadBootSector();

            Assert.AreEqual(bytesPerSector, Volume.BytesPerSector);
            Assert.AreEqual(Volume.BytesPerFileRecord / Volume.BytesPerSector, Volume.SectorsPerMFTRecord);
        }

        [Test]
        public void TestBytesPerSectorZero()
        {
            const ushort bytesPerSector = 0;

            BootSector.DummyBootSector.BytesPerSector = bytesPerSector;

            var ex = Assert.Throws<InvalidBootSectorException>(Volume.ReadBootSector);
            Assert.AreEqual(nameof(BootSector.DummyBootSector.BytesPerSector), ex.FieldName);
        }

        [Test]
        public void TestBytesPerSectorNotMultiple()
        {
            const ushort bytesPerSector = 511;

            BootSector.DummyBootSector.BytesPerSector = bytesPerSector;

            var ex = Assert.Throws<InvalidBootSectorException>(Volume.ReadBootSector);
            Assert.AreEqual(nameof(BootSector.DummyBootSector.BytesPerSector), ex.FieldName);
        }

        [Test]
        public void TestBytesPerSectorOverflow()
        {
            const ushort bytesPerSector = 8096;

            BootSector.DummyBootSector.BytesPerSector = bytesPerSector;

            var ex = Assert.Throws<InvalidBootSectorException>(Volume.ReadBootSector);
            Assert.AreEqual(nameof(BootSector.DummyBootSector.BytesPerSector), ex.FieldName);
        }

        [Test]
        public void TestSectorsPerClusterValid()
        {
            const byte sectorsPerCluster = 8;

            BootSector.DummyBootSector.SectorsPerCluster = sectorsPerCluster;

            Volume.ReadBootSector();

            Assert.AreEqual(sectorsPerCluster, Volume.SectorsPerCluster);
        }

        [Test]
        public void TestSectorsPerClusterMFTValid()
        {
            const byte sectorsPerCluster = 8;
            const byte clustersPerMftRecord = 10;

            BootSector.DummyBootSector.ClustersPerMFTRecord = clustersPerMftRecord;
            BootSector.DummyBootSector.SectorsPerCluster = sectorsPerCluster;

            Volume.ReadBootSector();

            Assert.AreEqual(clustersPerMftRecord * sectorsPerCluster * BootSector.DummyBootSector.BytesPerSector,
                Volume.BytesPerFileRecord);
        }
    }
}
