using System;
using System.Linq;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Tests.Driver;
using NtfsSharp.Tests.Driver.Attributes;
using NtfsSharp.Tests.Driver.Attributes.NonResident;
using NUnit.Framework;

namespace NtfsSharp.Tests.Attributes
{
    [TestFixture]
    public class TestData : TestAttributesBase
    {
        // Test the file record FileStream
        [Test]
        public void TestFileStream([Range(4096, 4096*10, 4096)] int byteLength)
        {
            var rand = new Random();
            var bytes = new byte[byteLength];

            rand.NextBytes(bytes);

            var nonResidentAttribute = new NonResidentData();
            nonResidentAttribute.AddDataAsVirtualClusters(bytes);
            DummyFileRecord.Attributes.Add(nonResidentAttribute);

            var actualFileRecord = ReadDummyFileRecord();

            var actualFileStream = actualFileRecord.FileStream;

            Assert.AreEqual(byteLength, actualFileStream.Length);
            Assert.True(actualFileStream.CanRead);
            Assert.False(actualFileStream.CanWrite);
            Assert.False(actualFileStream.EndOfFile);
            
            Assert.That(() =>
            {
                var actualBytes = new byte[actualFileStream.Length];
                actualFileStream.Read(actualBytes, 0, actualBytes.Length);

                return bytes.SequenceEqual(actualBytes);
            });
        }
        
        // Test data that is scattered in different clusters
        [Test]
        public void TestFragmentedData()
        {
            var nonResidentAttribute = new NonResidentData();

            var dataCluster1 = new DataCluster();
            dataCluster1.Data[0] = 1;
            dataCluster1.Data[4095] = 4;
            nonResidentAttribute.AppendVirtualCluster(dataCluster1, 100, 4);

            var dataCluster2 = new DataCluster();
            dataCluster2.Data[0] = 2;
            dataCluster2.Data[4095] = 3;
            nonResidentAttribute.AppendVirtualCluster(dataCluster2, 100, 5);

            var dataCluster3 = new DataCluster();
            dataCluster3.Data[0] = 3;
            dataCluster3.Data[4095] = 2;
            nonResidentAttribute.AppendVirtualCluster(dataCluster3, 100, 10);

            var dataCluster4 = new DataCluster();
            dataCluster4.Data[0] = 4;
            dataCluster4.Data[4095] = 1;
            nonResidentAttribute.AppendVirtualCluster(dataCluster4, 100, 13);

            DummyFileRecord.Attributes.Add(nonResidentAttribute);

            var actualFileRecord = ReadDummyFileRecord();
            var actualDataAttribute =
                (FileRecords.Attributes.DataAttribute) actualFileRecord.FindAttributeBodyByType(AttributeHeaderBase
                    .NTFS_ATTR_TYPE.DATA);

            var actualBytes = actualDataAttribute.Header.ReadBody();

            Assert.AreEqual(4 * DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster, actualBytes.Length);

            Assert.AreEqual(1, actualBytes[0 * DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster]);
            Assert.AreEqual(4, actualBytes[0 * DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster + 4095]);

            Assert.AreEqual(2, actualBytes[1 * DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster]);
            Assert.AreEqual(3, actualBytes[1 * DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster + 4095]);

            Assert.AreEqual(3, actualBytes[2 * DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster]);
            Assert.AreEqual(2, actualBytes[2 * DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster + 4095]);

            Assert.AreEqual(4, actualBytes[3 * DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster]);
            Assert.AreEqual(1, actualBytes[3 * DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster + 4095]);
        }

        [Test]
        public void TestResidentData()
        {
            var residentData = new ResidentData();

            residentData.Body[0] = 0xbe;
            residentData.Body[residentData.Body.Length - 1] = 0xef;

            DummyFileRecord.Attributes.Add(residentData);

            var actualFileRecord = ReadDummyFileRecord();
            var actualDataAttribute =
                (FileRecords.Attributes.DataAttribute)actualFileRecord.FindAttributeBodyByType(AttributeHeaderBase
                    .NTFS_ATTR_TYPE.DATA);

            var actualBytes = actualDataAttribute.Header.ReadBody();

            Assert.AreEqual(residentData.Body.Length, actualBytes.Length);

            Assert.AreEqual(0xbe, actualBytes[0]);
            Assert.AreEqual(0xef, actualBytes[actualBytes.Length - 1]);
        }
    }

    public class ResidentData : ResidentAttributeBase
    {
        public readonly byte[] Body;

        public ResidentData()
        {
            Header.Type = NTFS_ATTR_TYPE.DATA;
            Body = new byte[BytesUsed];
        }

        protected override byte[] GetBody()
        {
            return Body;
        }
    }

    public class NonResidentData : NonResidentAttributeBase
    {
        public NonResidentData()
        {
            Header.Type = NTFS_ATTR_TYPE.DATA;
        }
    }
}
