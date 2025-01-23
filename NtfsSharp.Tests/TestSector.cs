using NUnit.Framework;
using System;
using NtfsSharp.Tests.Driver;
using NtfsSharp.Units;
using NUnit.Framework.Legacy;

namespace NtfsSharp.Tests
{
    [TestFixture]
    public class TestSector
    {
        private DummyDriver Driver { get; set; }
        private Volume Volume { get; set; }

        [SetUp]
        public void SetUpDummyDisk()
        {
            Driver = new DummyDriver();
            Volume = new Volume(Driver);
        }

        [TearDown]
        public void DisposeDummyDisk()
        {
            Volume.Dispose();
        }

        /// <summary>
        /// Tests that <seealso cref="ArgumentNullException"/> is thrown from passing null as volume
        /// </summary>
        [Test]
        public void TestSectorVolumeNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                var sector = new Sector(0, (Volume) null);
            });

            ClassicAssert.AreEqual("vol", ex.ParamName);
        }

        /// <summary>
        /// Tests that <seealso cref="ArgumentNullException"/> is thrown from passing null as the byte array
        /// </summary>
        [Test]
        public void TestSectorDataNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                var sector = new Sector(0, (byte[]) null);
            });

            ClassicAssert.AreEqual("data", ex.ParamName);
        }

        /// <summary>
        /// Tests that <seealso cref="ArgumentOutOfRangeException"/> is thrown from passing a byte array smaller than 512 bytes
        /// </summary>
        [Test]
        public void TestSectorDataSizeTooSmall()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var sector = new Sector(0, new byte[1]);
            });

            ClassicAssert.AreEqual("data", ex.ParamName);
        }

        /// <summary>
        /// Tests that <seealso cref="ArgumentOutOfRangeException"/> is thrown from passing a byte array bigger than 512 bytes
        /// </summary>
        [Test]
        public void TestSectorDataSizeTooLarge()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var sector = new Sector(0, new byte[1024]);
            });

            ClassicAssert.AreEqual("data", ex.ParamName);
        }

        /// <summary>
        /// Tests that a byte array is stored in sector
        /// </summary>
        [Test]
        public void TestSectorDataSizeEqual()
        {
            var sectorData = new byte[512];
            var sector = new Sector(0, sectorData);

            ClassicAssert.AreSame(sectorData, sector.Data);
        }
        
        /// <summary>
        /// Tests that a <seealso cref="Guid"/> is read from a sector using a byte array
        /// </summary>
        [Test]
        public void TestSectorReadFileData()
        {
            const int sectorOffset = 256;

            var sectorBytes = new byte[512];

            var expected = new Guid("b5e04385-6ceb-4a88-a98d-87b019b6c756");
            var expectedBytes = expected.ToByteArray();

            Array.Copy(expectedBytes, 0, sectorBytes, sectorOffset, expectedBytes.Length);

            var sector = new Sector(0, sectorBytes);
            var actual = sector.ReadFile<Guid>(sectorOffset);

            ClassicAssert.AreEqual(expected, actual, "Actual GUID is different than expected GUID.");
            ClassicAssert.AreEqual(expectedBytes, actual.ToByteArray(), "Actual bytes is not same as expected bytes.");
        }

        /// <summary>
        /// Tests that a <seealso cref="Guid"/> is read from a sector using a <seealso cref="Volume"/>
        /// </summary>
        [Test]
        public void TestSectorReadFileVolume()
        {
            const int sectorOffset = 256;
            const uint lcn = 1;

            var dataCluster = new DataCluster();

            Driver.Clusters.Add(lcn, dataCluster);

            var expected = new Guid("b5e04385-6ceb-4a88-a98d-87b019b6c756");
            var expectedBytes = expected.ToByteArray();

            Array.Copy(expectedBytes, 0, dataCluster.Data, sectorOffset, expectedBytes.Length);

            var sector = new Sector(lcn * Volume.SectorsPerCluster, Volume);
            var actual = sector.ReadFile<Guid>(sectorOffset);

            ClassicAssert.AreEqual(expected, actual, "Actual GUID is different than expected GUID.");
            ClassicAssert.AreEqual(expectedBytes, actual.ToByteArray(), "Actual bytes is not same as expected bytes.");
        }
    }
}
