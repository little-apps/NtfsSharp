using System;
using System.Collections.Generic;
using System.Linq;
using NtfsSharp.Exceptions;
using NtfsSharp.Tests.Driver;
using NtfsSharp.Volumes;
using NUnit.Framework;

namespace NtfsSharp.Tests.MasterFileTable
{
    [TestFixture]
    public class TestMasterFileTable
    {
        public const ushort BytesPerSector = 512;
        public const byte SectorsPerCluster = 8;
        public ulong MftStartLcn => Volume.BootSector.BootSectorStructure.MFTLCN;
        public uint BytesPerFileRecord => (uint) Math.Pow(2, 256 - BootSector.DummyBootSector.ClustersPerMFTRecord);

        private uint _masterFileTableEntries = 26;

        private DummyDriver Driver { get; set; }
        private Volume Volume { get; set; }
        private BootSector BootSector { get; set; }
        private readonly List<MasterFileTableCluster> MasterFileTableParts = new List<MasterFileTableCluster>();

        [SetUp]
        public void SetUpDummyDisk()
        {
            Driver = new DummyDriver();
            Volume = new Volume(Driver, false);
            BootSector = new BootSector();

            Driver.Clusters.Add(0, BootSector);

            Volume.ReadBootSector();

            var bytesPerCluster = BytesPerSector * SectorsPerCluster;
            
            var neededMftClusters =
                Math.Ceiling((decimal) _masterFileTableEntries * BytesPerFileRecord / bytesPerCluster);

            for (var lcn = MftStartLcn; lcn < MftStartLcn + neededMftClusters; lcn++)
            {
                var mftPart = new MasterFileTableCluster(Driver, (uint) (bytesPerCluster / BytesPerFileRecord),
                    BytesPerFileRecord, (uint) lcn);
                
                MasterFileTableParts.Add(mftPart);
                Driver.Clusters.Add((long) lcn, mftPart);
            }
        }

        [TearDown]
        public void DisposeDummyDisk()
        {
            Volume.Dispose();
        }
        
        [Test]
        public void TestReadMft()
        {
            foreach (var mftPart in MasterFileTableParts)
            {
                mftPart.UseUpdateSequenceArray = true;
                mftPart.EndTag = 0xcafe;
                mftPart.FixUps = new ushort[] {0xfeed, 0xfeed};
            }

            Volume.ReadMft();

            Assert.AreEqual(_masterFileTableEntries, Volume.MFT.Count);
            Assert.That(() => Volume.MFT[0].Header.Magic.SequenceEqual(new byte[] {0x46, 0x49, 0x4c, 0x45}));
        }

        [Test]
        public void TestValidMftNums()
        {
            foreach (var mftPart in MasterFileTableParts)
            {
                mftPart.UseUpdateSequenceArray = true;
                mftPart.EndTag = 0xcafe;
                mftPart.FixUps = new ushort[] { 0xfeed, 0xfeed };

                for (var i = 0; i < mftPart.FilesPerPart; i++)
                {
                    // If the outer loops index is i and the inner loop index is j, this is essentially the same as (i * mftPart.FilesPerPart) + j
                    var mftNum = (uint) ((mftPart.Lcn - MftStartLcn) * mftPart.FilesPerPart + (ulong) i);
                    mftPart.FileRecords[i] = DummyFileRecord.BuildDummyFileRecord(mftNum);
                }
            }

            Volume.ReadMft();

            Assert.AreEqual(_masterFileTableEntries, Volume.MFT.Count);

            for (var i = 0; i < Volume.MFT.Count; i++)
            {
                var mftEntry = Volume.MFT[(uint) i];
                Assert.AreEqual(i, mftEntry.Header.MFTRecordNumber);
            }
        }

        [Test]
        public void TestInvalidMftNums()
        {
            foreach (var mftPart in MasterFileTableParts)
            {
                mftPart.UseUpdateSequenceArray = true;
                mftPart.EndTag = 0xcafe;
                mftPart.FixUps = new ushort[] { 0xfeed, 0xfeed };

                for (var i = 0; i < mftPart.FilesPerPart; i++)
                {
                    // Go backwards with MFT numbers (26, 25, ...)
                    var mftNum = (uint) (_masterFileTableEntries - (mftPart.Lcn - MftStartLcn) * mftPart.FilesPerPart + (ulong) i);
                    mftPart.FileRecords[i] = DummyFileRecord.BuildDummyFileRecord(mftNum);
                }
            }

            var ex =
                Assert.Throws(typeof(InvalidMasterFileTableException), () => Volume.ReadMft()) as
                    InvalidMasterFileTableException;
            Assert.AreEqual(nameof(DummyFileRecord.FILE_RECORD_HEADER_NTFS.MFTRecordNumber), ex.ParamName);
        }

        [Test]
        public void TestReadsAllExceptInMiddle()
        {
            var prevClusterIndex = -1;

            LoopThroughMft((cluster, mftVirtualClusterIndex, mftIndex, mftPartIndex) =>
            {
                if (prevClusterIndex != mftVirtualClusterIndex)
                {
                    // Set USA for MFT cluster
                    cluster.UseUpdateSequenceArray = true;
                    cluster.EndTag = 0xcafe;
                    cluster.FixUps = new ushort[] { 0xfeed, 0xfeed };

                    prevClusterIndex = mftVirtualClusterIndex;
                }

                if (mftIndex != _masterFileTableEntries / 2)
                    cluster.FileRecords[mftPartIndex] = DummyFileRecord.BuildDummyFileRecord((uint) mftIndex);
            });

            Volume.ReadMft();

            Assert.AreEqual(_masterFileTableEntries - 1, Volume.MFT.Count);

            for (var i = 0; i < Volume.MFT.Count; i++)
            {
                var mftEntry = Volume.MFT[(uint) i];
                Assert.AreEqual(i, mftEntry.Header.MFTRecordNumber);
            }
        }

        [Test]
        public void TestReadsAllExceptAtEnd()
        {
            var prevClusterIndex = -1;

            LoopThroughMft((cluster, mftVirtualClusterIndex, mftIndex, mftPartIndex) =>
            {
                if (prevClusterIndex != mftVirtualClusterIndex)
                {
                    // Set USA for MFT cluster
                    cluster.UseUpdateSequenceArray = true;
                    cluster.EndTag = 0xcafe;
                    cluster.FixUps = new ushort[] { 0xfeed, 0xfeed };

                    prevClusterIndex = mftVirtualClusterIndex;
                }

                if (mftIndex != _masterFileTableEntries - 1)
                    cluster.FileRecords[mftPartIndex] = DummyFileRecord.BuildDummyFileRecord((uint) mftIndex);
            });

            Volume.ReadMft();

            Assert.AreEqual(_masterFileTableEntries - 1, Volume.MFT.Count);

            for (var i = 0; i < Volume.MFT.Count; i++)
            {
                var mftEntry = Volume.MFT[(uint) i];
                Assert.AreEqual(i, mftEntry.Header.MFTRecordNumber);
            }
        }

        /// <summary>
        /// Loops through the MFT entries, calling a action with the cluster, virtual cluster index, entry index, and part index (in cluster) in that order.
        /// </summary>
        /// <param name="action">Callback action</param>
        private void LoopThroughMft(Action<MasterFileTableCluster, int, uint, long> action)
        {
            var mftVirtualClusterIndex = 0;
            var mftCluster = MasterFileTableParts[mftVirtualClusterIndex];

            for (uint mftIndex = 0, mftPartIndex = 0; mftIndex < _masterFileTableEntries; mftIndex++, mftPartIndex++)
            {
                if (mftIndex * BytesPerFileRecord % (BytesPerSector * SectorsPerCluster) == 0 && mftIndex > 0)
                {
                    // On to the next cluster
                    mftVirtualClusterIndex++;
                    mftCluster = MasterFileTableParts[mftVirtualClusterIndex];

                    // Reset part index to 0
                    mftPartIndex = 0;
                }

                action(mftCluster, mftVirtualClusterIndex, mftIndex, mftPartIndex);
            }
        }
    }
}
