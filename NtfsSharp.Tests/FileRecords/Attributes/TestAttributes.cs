using System;
using NtfsSharp.Exceptions;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.Base.NonResident;
using NtfsSharp.Tests.Driver;
using NtfsSharp.Tests.Driver.Attributes;
using NtfsSharp.Tests.Driver.Attributes.NonResident;
using NUnit.Framework;

namespace NtfsSharp.Tests.FileRecords.Attributes
{
    [TestFixture]
    public class TestAttributes : TestFileRecordBase
    {
        public static DummyAttributeBase.NTFS_ATTR_TYPE[] ResidentTypes =
        {
            DummyAttributeBase.NTFS_ATTR_TYPE.BITMAP,
            DummyAttributeBase.NTFS_ATTR_TYPE.DATA,
            DummyAttributeBase.NTFS_ATTR_TYPE.EA,
            DummyAttributeBase.NTFS_ATTR_TYPE.EA_INFORMATION,
            DummyAttributeBase.NTFS_ATTR_TYPE.FILE_NAME,
            DummyAttributeBase.NTFS_ATTR_TYPE.LOGGED_UTILITY_STREAM,
            DummyAttributeBase.NTFS_ATTR_TYPE.OBJECT_ID,
            DummyAttributeBase.NTFS_ATTR_TYPE.REPARSE_POINT,
            DummyAttributeBase.NTFS_ATTR_TYPE.SECURITY_DESCRIPTOR,
            DummyAttributeBase.NTFS_ATTR_TYPE.STANDARD_INFORMATION,
            DummyAttributeBase.NTFS_ATTR_TYPE.VOLUME_INFORMATION,
            DummyAttributeBase.NTFS_ATTR_TYPE.VOLUME_NAME
        };

        public static DummyAttributeBase.NTFS_ATTR_TYPE[] NonResidentTypes =
        {
            DummyAttributeBase.NTFS_ATTR_TYPE.BITMAP,
            DummyAttributeBase.NTFS_ATTR_TYPE.DATA,
            DummyAttributeBase.NTFS_ATTR_TYPE.EA,
            DummyAttributeBase.NTFS_ATTR_TYPE.EA_INFORMATION,
            DummyAttributeBase.NTFS_ATTR_TYPE.LOGGED_UTILITY_STREAM,
            DummyAttributeBase.NTFS_ATTR_TYPE.OBJECT_ID,
            DummyAttributeBase.NTFS_ATTR_TYPE.REPARSE_POINT,
            DummyAttributeBase.NTFS_ATTR_TYPE.SECURITY_DESCRIPTOR,
            DummyAttributeBase.NTFS_ATTR_TYPE.VOLUME_INFORMATION
        };

        [Test]
        public void TestResidentAttribute([ValueSource(nameof(ResidentTypes))] DummyAttributeBase.NTFS_ATTR_TYPE attrType)
        {
            var residentAttr = new ResidentDummy(attrType);

            DummyFileRecord.Attributes.Add(residentAttr);

            var actualFileRecord = ReadDummyFileRecord();

            Assert.AreEqual(1, actualFileRecord.Attributes.Count);

            var actualAttribute = actualFileRecord.Attributes[0];
            var actualBody = actualAttribute.Body.Body ?? actualAttribute.Header.ReadBody();

            Assert.AreEqual((uint)attrType, (uint)actualAttribute.Header.Header.Type);
            Assert.False(actualAttribute.Header.Header.NonResident);
            Assert.AreEqual(actualBody.Length, actualBody.Length);
        }

        [Test]
        public void TestInvalidResidentAttributeType()
        {
            var residentAttr = new ResidentDummy(0);

            DummyFileRecord.Attributes.Add(residentAttr);

            Assert.Throws<InvalidAttributeException>(() => ReadDummyFileRecord());
        }

        [Test]
        public void TestNonResidentAttribute([ValueSource(nameof(NonResidentTypes))] DummyAttributeBase.NTFS_ATTR_TYPE attrType)
        {
            var nonResidentAttr = new NonResidentDummy(attrType);
            var dataCluster = new DataCluster();

            nonResidentAttr.AppendVirtualCluster(dataCluster);

            DummyFileRecord.Attributes.Add(nonResidentAttr);

            var actualFileRecord = ReadDummyFileRecord();

            Assert.AreEqual(1, actualFileRecord.Attributes.Count);

            var actualAttribute = actualFileRecord.Attributes[0];
            var actualBody = actualAttribute.Body.Body ?? actualAttribute.Header.ReadBody();

            Assert.AreEqual((uint) attrType, (uint) actualAttribute.Header.Header.Type);
            Assert.True(actualAttribute.Header.Header.NonResident);
            Assert.AreEqual(dataCluster.Data.Length, actualBody.Length);
        }

        [Test]
        public void TestInvalidNonResidentAttributeType()
        {
            var nonResidentAttr = new NonResidentDummy(0);

            DummyFileRecord.Attributes.Add(nonResidentAttr);

            Assert.Throws<InvalidAttributeException>(() => ReadDummyFileRecord());
        }

        [Test]
        public void TestNonResidentNegativeLcn([ValueSource(nameof(NonResidentTypes))] DummyAttributeBase.NTFS_ATTR_TYPE attrType)
        {
            var nonResidentAttr = new NonResidentDummy(attrType);
            var dataClusters = new[]
            {
                new DataCluster(),
                new DataCluster()
            };
            
            var rand = new Random();

            var firstLcnOffset = rand.Next(1, (int) ((int) DummyDriver.DriveSize / DummyDriver.BytesPerSector / DummyDriver.SectorsPerCluster));
            var secondLcnOffset = rand.Next(1, firstLcnOffset);

            nonResidentAttr.AppendVirtualCluster(dataClusters[0], (ulong) firstLcnOffset);
            nonResidentAttr.AppendVirtualCluster(dataClusters[1], (ulong) secondLcnOffset);

            DummyFileRecord.Attributes.Add(nonResidentAttr);

            var actualFileRecord = ReadDummyFileRecord();

            var actualAttribute = actualFileRecord.Attributes[0];
            var actualNonResidentAttr = actualAttribute.Header as NonResident;
            var actualBody = actualAttribute.Body.Body ?? actualAttribute.Header.ReadBody();

            Assert.AreEqual((uint)attrType, (uint)actualAttribute.Header.Header.Type);
            Assert.True(actualAttribute.Header.Header.NonResident);
            Assert.False(actualNonResidentAttr.DataBlocks[0].LcnOffsetNegative);
            Assert.True(actualNonResidentAttr.DataBlocks[1].LcnOffsetNegative);

            Assert.AreEqual(secondLcnOffset, actualNonResidentAttr.VcnToLcn(1));
            
        }

        [Test]
        public void TestNonResidentNegativeFirstLcn([ValueSource(nameof(NonResidentTypes))] DummyAttributeBase.NTFS_ATTR_TYPE attrType)
        {
            var nonResidentAttr = new NonResidentDummy(attrType);
            var dataCluster = new DataCluster();

            var rand = new Random();
            var negativeLcnOffset = rand.Next(int.MinValue, -1);

            nonResidentAttr.AppendVirtualCluster(dataCluster, (ulong) negativeLcnOffset);

            DummyFileRecord.Attributes.Add(nonResidentAttr);
            
            var actualException = Assert.Catch(() =>
            {
                var actualFileRecord = ReadDummyFileRecord();

                // If above didn't trigger exception, it's probably because the body wasn't read.
                var actualAttribute = actualFileRecord.FindAttributeByType((AttributeHeaderBase.NTFS_ATTR_TYPE)attrType);

                actualAttribute.Header.ReadBody();
            });

            // TargetInvocationException may have been thrown, causing the actual exception to be the inner exception.
            if (actualException.InnerException != null)
                actualException = actualException.InnerException;

            Assert.IsInstanceOf<InvalidAttributeException>(actualException);
        }

        [Test]
        public void TestNonResidentDataRunLengthSize()
        {
            
        }

        [Test]
        public void TestNonResidentDataRunOffsetSize()
        {
            
        }
    }

    public class ResidentDummy : ResidentAttributeBase
    {
        public readonly byte[] Body;

        public ResidentDummy(NTFS_ATTR_TYPE type)
        {
            Header.Type = type;
            Body = new byte[128];
        }

        protected override byte[] GetBody()
        {
            return Body;
        }
    }

    public class NonResidentDummy : NonResidentAttributeBase
    {
        public NonResidentDummy(NTFS_ATTR_TYPE type)
        {
            Header.Type = type;
        }
    }
}
