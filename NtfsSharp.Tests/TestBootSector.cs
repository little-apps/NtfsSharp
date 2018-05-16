using NUnit.Framework;
using System;
using NtfsSharp.Exceptions;
using NtfsSharp.Tests.Driver;
using NtfsSharp.Volumes;

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

            Driver.Clusters[0] = BootSector;
        }

        [TearDown]
        public void DisposeDummyDisk()
        {
            Volume.Dispose();
        }

        /// <summary>
        /// Tests clusters per MFT as valid positive value (0x7F or 127)
        /// </summary>
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

        /// <summary>
        /// Tests clusters per MFT as valid negative value (0xF6 or -10)
        /// </summary>
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

        /// <summary>
        /// Tests that <seealso cref="InvalidBootSectorException"/> is thrown from setting clusters per MFT to an invalid negative value (0x80 or -128)
        /// </summary>
        [Test]
        public void TestClustersPerMFTRecordNegativeInvalid()
        {
            var clustersPerMftRecord = unchecked((sbyte)0x80);

            BootSector.DummyBootSector.ClustersPerMFTRecord = (byte)clustersPerMftRecord;

            var ex = Assert.Throws<InvalidBootSectorException>(Volume.ReadBootSector);
            Assert.AreEqual(nameof(BootSector.DummyBootSector.ClustersPerMFTRecord), ex.FieldName);
        }

        /// <summary>
        /// Tests that the bytes per sector is set to a valid value (512)
        /// </summary>
        [Test]
        public void TestBytesPerSectorValid()
        {
            const ushort bytesPerSector = 512;

            BootSector.DummyBootSector.BytesPerSector = bytesPerSector;

            Volume.ReadBootSector();

            Assert.AreEqual(bytesPerSector, Volume.BytesPerSector);
            Assert.AreEqual(Volume.BytesPerFileRecord / Volume.BytesPerSector, Volume.SectorsPerMftRecord);
        }

        /// <summary>
        /// Tests that <seealso cref="InvalidBootSectorException"/> is thrown from setting bytes per sector to 0
        /// </summary>
        [Test]
        public void TestBytesPerSectorZero()
        {
            const ushort bytesPerSector = 0;

            BootSector.DummyBootSector.BytesPerSector = bytesPerSector;

            var ex = Assert.Throws<InvalidBootSectorException>(Volume.ReadBootSector);
            Assert.AreEqual(nameof(BootSector.DummyBootSector.BytesPerSector), ex.FieldName);
        }

        /// <summary>
        /// Tests that <seealso cref="InvalidBootSectorException"/> is thrown from setting bytes per sectors to something not a multiple of 512
        /// </summary>
        [Test]
        public void TestBytesPerSectorNotMultiple()
        {
            const ushort bytesPerSector = 511;

            BootSector.DummyBootSector.BytesPerSector = bytesPerSector;

            var ex = Assert.Throws<InvalidBootSectorException>(Volume.ReadBootSector);
            Assert.AreEqual(nameof(BootSector.DummyBootSector.BytesPerSector), ex.FieldName);
        }

        /// <summary>
        /// Tests that <seealso cref="InvalidBootSectorException"/> is thrown from setting the bytes per sector to greater than 4096
        /// </summary>
        [Test]
        public void TestBytesPerSectorOverflow()
        {
            const ushort bytesPerSector = 8096;

            BootSector.DummyBootSector.BytesPerSector = bytesPerSector;

            var ex = Assert.Throws<InvalidBootSectorException>(Volume.ReadBootSector);
            Assert.AreEqual(nameof(BootSector.DummyBootSector.BytesPerSector), ex.FieldName);
        }

        /// <summary>
        /// Tests setting the sectors per cluster to 8 is valid
        /// </summary>
        [Test]
        public void TestSectorsPerClusterValid()
        {
            const byte sectorsPerCluster = 8;

            BootSector.DummyBootSector.SectorsPerCluster = sectorsPerCluster;

            Volume.ReadBootSector();

            Assert.AreEqual(sectorsPerCluster, Volume.SectorsPerCluster);
        }

        /// <summary>
        /// Tests setting the sectors per cluster is reflected in the bytes per file record value (when clusters per MFT is a positive value)
        /// </summary>
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
