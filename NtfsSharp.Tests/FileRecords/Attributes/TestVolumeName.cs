using System;
using System.Text;
using NtfsSharp.Files.Attributes;
using NtfsSharp.Files.Attributes.Base;
using NtfsSharp.Tests.Driver.Attributes;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace NtfsSharp.Tests.FileRecords.Attributes
{
    [TestFixture]
    public class TestVolumeName : TestFileRecordBase
    {
        /// <summary>
        /// Tests a simple volume name is read correctly.
        /// </summary>
        [Test]
        public void TestValidVolumeName()
        {
            const string expectedVolumeName = "Foo Bar";

            var volumeNameAttr = new VolumeNameDummy {VolumeName = expectedVolumeName};
            
            DummyFileRecord.Attributes.Add(volumeNameAttr);

            var actualFileRecord = ReadDummyFileRecord();

            ClassicAssert.AreEqual(1, actualFileRecord.Attributes.Count);

            var actualAttribute = actualFileRecord.Attributes[0];
            
            ClassicAssert.AreEqual(AttributeHeaderBase.NTFS_ATTR_TYPE.VOLUME_NAME, actualAttribute.Header.Header.Type);

            var actualVolumeNameAttr = actualAttribute.Body as VolumeName;
            ClassicAssert.NotNull(actualVolumeNameAttr);
            ClassicAssert.AreEqual(expectedVolumeName, actualVolumeNameAttr.Name);
        }

        /// <summary>
        /// Tests the length of the volume name is the same as the size of the body of the resident attribute.
        /// </summary>
        /// <param name="expectedVolumeNameLength">The expected volume name length.</param>
        /// <remarks>The length cannot be more than 940 cause 1024 (size of the file record) minus 56 bytes for file record structure and where the attributes start, 16 + 8 for the attribute header and 4 for the end marker.</remarks>
        [Test]
        public void TestValidVolumeNameValidSize([Random(1, 940, 10)] int expectedVolumeNameLength)
        {
            var volumeNameAttr = new VolumeNameDummy {Body = new byte[expectedVolumeNameLength] };

            DummyFileRecord.Attributes.Add(volumeNameAttr);

            var actualFileRecord = ReadDummyFileRecord();

            ClassicAssert.AreEqual(1, actualFileRecord.Attributes.Count);

            var actualAttribute = actualFileRecord.Attributes[0];

            ClassicAssert.AreEqual(AttributeHeaderBase.NTFS_ATTR_TYPE.VOLUME_NAME, actualAttribute.Header.Header.Type);

            var actualVolumeNameAttr = actualAttribute.Body as VolumeName;
            ClassicAssert.NotNull(actualVolumeNameAttr);
            ClassicAssert.AreEqual(expectedVolumeNameLength, actualVolumeNameAttr.Body.Length);
        }

        /// <summary>
        /// Tests the length of the volume name is too big for resident attribute.
        /// </summary>
        /// <param name="expectedVolumeNameLength">The expected volume name length.</param>
        [Test]
        public void TestValidVolumeNameInvalidSize([Random(940, 0xFFFF, 10)] ushort expectedVolumeNameLength)
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
