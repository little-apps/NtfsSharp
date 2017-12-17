using System;
using System.Text;
using NtfsSharp.FileRecords.Attributes;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Tests.Driver.Attributes;
using NUnit.Framework;

namespace NtfsSharp.Tests.FileRecords.Attributes
{
    [TestFixture]
    public class TestVolumeName : TestFileRecordBase
    {
        [Test]
        public void TestValidVolumeName()
        {
            const string expectedVolumeName = "Foo Bar";

            var volumeNameAttr = new VolumeNameDummy {VolumeName = expectedVolumeName};
            
            DummyFileRecord.Attributes.Add(volumeNameAttr);

            var actualFileRecord = ReadDummyFileRecord();

            Assert.AreEqual(1, actualFileRecord.Attributes.Count);

            var actualAttribute = actualFileRecord.Attributes[0];
            
            Assert.AreEqual(AttributeHeaderBase.NTFS_ATTR_TYPE.VOLUME_NAME, actualAttribute.Header.Header.Type);

            var actualVolumeNameAttr = actualAttribute.Body as VolumeName;
            Assert.NotNull(actualVolumeNameAttr);
            Assert.AreEqual(expectedVolumeName, actualVolumeNameAttr.Name);
        }

        /// <summary>
        /// Tests the length of the volume name is the same as the size of the body of the resident attribute.
        /// </summary>
        /// <param name="expectedVolumeNameLength">The expected volume name length.</param>
        /// <remarks>This is a resident attribute so the length cannot be more than the size of the file record minus 48 bytes for the file record structure.</remarks>
        [Test]
        public void TestValidVolumeNameValidSize([Random(1, 1024 - 48, 10)] int expectedVolumeNameLength)
        {
            var volumeNameAttr = new VolumeNameDummy {Body = new byte[expectedVolumeNameLength] };

            DummyFileRecord.Attributes.Add(volumeNameAttr);

            var actualFileRecord = ReadDummyFileRecord();

            Assert.AreEqual(1, actualFileRecord.Attributes.Count);

            var actualAttribute = actualFileRecord.Attributes[0];

            Assert.AreEqual(AttributeHeaderBase.NTFS_ATTR_TYPE.VOLUME_NAME, actualAttribute.Header.Header.Type);

            var actualVolumeNameAttr = actualAttribute.Body as VolumeName;
            Assert.NotNull(actualVolumeNameAttr);
            Assert.AreEqual(expectedVolumeNameLength, actualVolumeNameAttr.Body.Length);
        }

        /// <summary>
        /// Tests the length of the volume name is too big for resident attribute.
        /// </summary>
        /// <param name="expectedVolumeNameLength">The expected volume name length.</param>
        [Test]
        public void TestValidVolumeNameInvalidSize([Random(1024 - 48, int.MaxValue, 10)] int expectedVolumeNameLength)
        {
            var volumeNameAttr = new VolumeNameDummy { Body = new byte[expectedVolumeNameLength] };

            DummyFileRecord.Attributes.Add(volumeNameAttr);

            Assert.Catch<IndexOutOfRangeException>(() => ReadDummyFileRecord());
        }
    }

    public class VolumeNameDummy : ResidentAttributeBase
    {
        public string VolumeName
        {
            get => Encoding.Unicode.GetString(Body);
            set => Body = Encoding.Unicode.GetBytes(value);
        }

        public byte[] Body;

        public VolumeNameDummy()
        {
            Header.Type = NTFS_ATTR_TYPE.VOLUME_NAME;
            Body = new byte[128];
        }

        protected override byte[] GetBody()
        {
            return Body;
        }
    }
}
