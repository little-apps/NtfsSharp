using System;
using NtfsSharp.Tests.Driver;
using NtfsSharp.Units;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace NtfsSharp.Tests
{
    [TestFixture]
    public class TestCluster
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
        /// Tests <seealso cref="ArgumentNullException"/> is thrown from <seealso cref="Volume"/> being null
        /// </summary>
        [Test]
        public void TestClusterVolumeNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                var cluster = new Cluster(0, null);
            });
            
            ClassicAssert.AreEqual("vol", ex.ParamName);
        }

        /// <summary>
        /// Tests the number of sectors in <seealso cref="Cluster"/> is the number of sectors per cluster
        /// </summary>
        [Test]
        public void TestClusterSectorsLength()
        {
            var cluster = new Cluster(1, Volume);

            ClassicAssert.AreEqual(DummyDriver.SectorsPerCluster, cluster.Sectors.Length);
        }

        /// <summary>
        /// Tests that a GUID is read from an offset in each of the sectors and an offset in the cluster
        /// </summary>
        [Test]
        public void TestClusterSectorsRead()
        {
            const uint lcn = 1;
            const uint sectorOffset = 256;
            
            var dataCluster = new DataCluster();

            Driver.Clusters.Add(lcn, dataCluster);

            for (var sectorIndex = 0; sectorIndex < DummyDriver.SectorsPerCluster; sectorIndex++)
            {
                var sectorExpected = Guid.NewGuid();
                var clusterOffset = (uint) sectorIndex * 512 + sectorOffset;

                Array.Copy(sectorExpected.ToByteArray(), 0, dataCluster.Data, clusterOffset,
                    sectorExpected.ToByteArray().Length);

                var cluster = Volume.ReadCluster(lcn);

                ClassicAssert.AreEqual(sectorExpected, cluster.ReadFile<Guid>(clusterOffset), $"GUID read at offset {clusterOffset} in cluster is different.");
                ClassicAssert.AreEqual(sectorExpected, cluster.Sectors[sectorIndex].ReadFile<Guid>(sectorOffset), $"GUID read in sector #{sectorExpected} is different.");
            }
            
        }

        /// <summary>
        /// Tests that <seealso cref="ArgumentOutOfRangeException"/> is thrown from trying to read from an invalid offset
        /// </summary>
        [Test]
        public void TestClusterSectorsReadInvalidOffset()
        {
            var cluster = Volume.ReadCluster(0);

            Assert.Throws<ArgumentOutOfRangeException>(() => cluster.ReadFile<Guid>(4090));
            Assert.Throws<ArgumentOutOfRangeException>(() => cluster.Sectors[0].ReadFile<Guid>(500));
        }
    }
}
