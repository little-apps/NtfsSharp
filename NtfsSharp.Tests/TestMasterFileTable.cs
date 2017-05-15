using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using NtfsSharp.Exceptions;
using NtfsSharp.Tests.Driver;

namespace NtfsSharp.Tests
{
    [TestFixture]
    public class TestMasterFileTable
    {
        public const ushort BytesPerSector = 512;
        public const byte SectorsPerCluster = 8;

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
            var bytesPerFileRecord = (uint) Math.Pow(2, 256 - BootSector.DummyBootSector.ClustersPerMFTRecord);

            for (uint lcn = 1; lcn < _masterFileTableEntries * bytesPerCluster / bytesPerCluster+1; lcn++)
            {
                var mftPart = new MasterFileTableCluster((uint) (bytesPerCluster / bytesPerFileRecord),
                    bytesPerFileRecord, lcn);
                
                MasterFileTableParts.Add(mftPart);
                Driver.Clusters.Add(lcn, mftPart);
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
                    var mftNum = (uint) ((mftPart.Lcn - 1) * mftPart.FilesPerPart + i);
                    mftPart.FileRecords.Add(DummyFileRecord.BuildDummyFileRecord(mftNum));
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
                    var mftNum = (uint) (_masterFileTableEntries - (mftPart.Lcn - 1) * mftPart.FilesPerPart + i);
                    mftPart.FileRecords.Add(DummyFileRecord.BuildDummyFileRecord(mftNum));
                }
            }

            var ex = Assert.Throws(typeof(InvalidMasterFileTableException), Volume.ReadMft) as InvalidMasterFileTableException;
            Assert.AreEqual(nameof(DummyFileRecord.FILE_RECORD_HEADER_NTFS.MFTRecordNumber), ex.ParamName);
        }
    }
}
