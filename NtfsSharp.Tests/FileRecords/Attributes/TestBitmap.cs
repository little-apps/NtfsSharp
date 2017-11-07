using System;
using System.Collections;
using System.Linq;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Tests.Driver;
using NtfsSharp.Tests.Driver.Attributes.NonResident;
using NUnit.Framework;

namespace NtfsSharp.Tests.FileRecords.Attributes
{
    [TestFixture]
    public class TestBitmap : TestFileRecordBase
    {
        [Test]
        public void TestReadBitmap()
        {
            var bitArray = new BitArray((int) (DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster));

            var bitmapAttr = new BitmapAttribute();
            bitmapAttr.AddBitArray(bitArray);
            DummyFileRecord.Attributes.Add(bitmapAttr);

            var actualFileRecord = ReadDummyFileRecord();

            Assert.AreEqual(1, actualFileRecord.Attributes.Count);
            Assert.AreEqual(AttributeHeaderBase.NTFS_ATTR_TYPE.BITMAP, actualFileRecord.Attributes[0].Header.Header.Type);
        }

        [Test, Repeat(2)]
        public void TestSameBitArray()
        {
            var expectedBitArray = new BitArray((int)(DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster) * 8);
            var random = new Random();

            for (var i = 0; i < expectedBitArray.Length; i++)
            {
                expectedBitArray[i] = random.Next() % 2 == 0;
            }

            var bitmapAttr = new BitmapAttribute();
            bitmapAttr.AddBitArray(expectedBitArray);
            DummyFileRecord.Attributes.Add(bitmapAttr);

            var actualFileRecord = ReadDummyFileRecord();
            var attributeBody = actualFileRecord.FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE.BITMAP);

            Assert.NotNull(attributeBody);

            var actualBitArray = new BitArray(attributeBody.Body);

            Assert.AreEqual(expectedBitArray.Length, actualBitArray.Length);
            // Check that all bits aren't different
            Assert.That(() => { return expectedBitArray.Xor(actualBitArray).Cast<bool>().All(bit => !bit); });
        }

        [Test]
        public void TestEmptyBitArray()
        {
            var expectedBitArray = new BitArray(0);

            var bitmapAttr = new BitmapAttribute();
            bitmapAttr.AddBitArray(expectedBitArray);
            DummyFileRecord.Attributes.Add(bitmapAttr);

            var actualFileRecord = ReadDummyFileRecord();
            var attributeBody = actualFileRecord.FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE.BITMAP);

            Assert.NotNull(attributeBody);
            
            // Should just be one cluster
            Assert.AreEqual(DummyDriver.BytesPerSector * DummyDriver.SectorsPerCluster, attributeBody.Body.Length);
            // Should all be zeroes
            Assert.That(() => { return new BitArray(attributeBody.Body).Cast<bool>().All(bit => !bit); });
        }
    }

    public class BitmapAttribute : NonResidentAttributeBase
    {
        public BitmapAttribute()
        {
            Header.Type = NTFS_ATTR_TYPE.BITMAP;
        }

        public ulong[] AddBitArray(BitArray bitArray, ulong startLcn = 100)
        {
            var bytes = new byte[(bitArray.Length - 1) / 8 + 1];
            bitArray.CopyTo(bytes, 0);

            return AddDataAsVirtualClusters(bytes, startLcn);
        }
    }
}
