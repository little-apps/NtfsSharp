using System;
using NtfsSharp.Exceptions;
using NtfsSharp.FileRecords;
using NUnit.Framework;

namespace NtfsSharp.Tests.FileRecords
{
    [TestFixture]
    public class TestFixupable : TestFileRecordBase
    {
        /// <summary>
        /// Test the right bytes at the end of each sector are replaced
        /// </summary>
        [Test]
        public void TestFileRecordUsaReplaced()
        {
            // The last two bytes expected at the end of each sector in file record.
            const ushort endTag = 0xabab;

            // These will be replaced with the last two bytes at the end of each sector in the file record.
            var expectedUsas = new ushort[] {0xcdcd, 0xefef};

            var fileRecordWithUsa = DummyFileRecord.BuildWithUsa(BytesPerFileRecord, Driver, endTag, expectedUsas);

            // Make sure last two bytes of each sector don't match expected USA (before it's parsed)
            for (var sectorIndex = 0;
                sectorIndex < BytesPerFileRecord / BootSector.DummyBootSector.BytesPerSector;
                sectorIndex++)
            {
                var actualUsa = BitConverter.ToUInt16(fileRecordWithUsa,
                    (sectorIndex + 1) * BootSector.DummyBootSector.BytesPerSector - 2);

                Assert.AreNotEqual(expectedUsas[sectorIndex], actualUsa);
            }

            var fileRecord = new FileRecord(fileRecordWithUsa, Volume);

            Assert.NotNull(fileRecord);

            var fileRecordParsed = fileRecord.Data;

            // Make sure last two bytes of each sector match expected USA (after it's parsed)
            for (var sectorIndex = 0;
                sectorIndex < BytesPerFileRecord / BootSector.DummyBootSector.BytesPerSector;
                sectorIndex++)
            {
                var actualUsa = BitConverter.ToUInt16(fileRecordParsed,
                    (sectorIndex + 1) * BootSector.DummyBootSector.BytesPerSector - 2);

                Assert.AreEqual(expectedUsas[sectorIndex], actualUsa);
            }
        }

        /// <summary>
        /// Test last two bytes of sector match the end tag
        /// </summary>
        [Test]
        public void TestEndTagMatches()
        {
            var fileRecordWithoutUsaBytes = DummyFileRecord.Build(BytesPerFileRecord, Driver);

            // The 2 bytes expected at the end of each sector
            var expectedEndTag = new byte[] {0xab, 0xcd};

            // The offset of the two bytes that should match the last two bytes of the sector
            var usaOffset = BitConverter.ToUInt16(fileRecordWithoutUsaBytes, 4);

            Array.Copy(expectedEndTag, 0, fileRecordWithoutUsaBytes, usaOffset, 2);

            var fileRecordWithUsa = fileRecordWithoutUsaBytes;

            // Set last two bytes of each sector in file record to end tag
            for (var sectorIndex = 0;
                sectorIndex < BytesPerFileRecord / BootSector.DummyBootSector.BytesPerSector;
                sectorIndex++)
            {
                Array.Copy(expectedEndTag, 0, fileRecordWithUsa,
                    (sectorIndex + 1) * BootSector.DummyBootSector.BytesPerSector - 2, 2);
            }

            for (var sectorIndex = 0;
                sectorIndex < BytesPerFileRecord / BootSector.DummyBootSector.BytesPerSector;
                sectorIndex++)
            {
                // Make sure last two bytes of each sector are set to end tag
                Assert.AreEqual(expectedEndTag[0],
                    fileRecordWithUsa[(sectorIndex + 1) * BootSector.DummyBootSector.BytesPerSector - 2]);
                Assert.AreEqual(expectedEndTag[1],
                    fileRecordWithUsa[(sectorIndex + 1) * BootSector.DummyBootSector.BytesPerSector - 1]);
            }

            // Tests if file record is parsed and end tags match
            FileRecord fileRecord = null;

            Assert.DoesNotThrow(() =>
            {
                fileRecord = new FileRecord(fileRecordWithUsa, Volume);
            });

            Assert.NotNull(fileRecord);
            Assert.AreEqual(expectedEndTag[0], fileRecord.EndTag[0]);
            Assert.AreEqual(expectedEndTag[1], fileRecord.EndTag[1]);
        }

        /// <summary>
        /// Test last two bytes of sector don't match the end tag 
        /// </summary>
        [Test]
        public void TestEndTagsDontMatch()
        {
            var fileRecordWithoutUsaBytes = DummyFileRecord.Build(BytesPerFileRecord, Driver);

            // The 2 bytes expected at the end of each sector
            var expectedEndTag = new byte[] {0xab, 0xcd};

            // The offset of the two bytes that should match the last two bytes of the sector
            var usaOffset = BitConverter.ToUInt16(fileRecordWithoutUsaBytes, 4);

            Array.Copy(expectedEndTag, 0, fileRecordWithoutUsaBytes, usaOffset, 2);

            var invalidEndTag = new byte[] {0xdc, 0xba};
            var fileRecordWithUsa = fileRecordWithoutUsaBytes;

            // Set last two bytes of each sector in file record to different end tag
            for (var sectorIndex = 0;
                sectorIndex < BytesPerFileRecord / BootSector.DummyBootSector.BytesPerSector;
                sectorIndex++)
            {
                Array.Copy(invalidEndTag, 0, fileRecordWithUsa,
                    (sectorIndex + 1) * BootSector.DummyBootSector.BytesPerSector - 2, 2);
            }

            for (var sectorIndex = 0;
                sectorIndex < BytesPerFileRecord / BootSector.DummyBootSector.BytesPerSector;
                sectorIndex++)
            {
                // Make sure last two bytes of each sector aren't set to expected end tag
                Assert.AreNotEqual(expectedEndTag[0],
                    fileRecordWithUsa[(sectorIndex + 1) * BootSector.DummyBootSector.BytesPerSector - 2]);
                Assert.AreNotEqual(expectedEndTag[1],
                    fileRecordWithUsa[(sectorIndex + 1) * BootSector.DummyBootSector.BytesPerSector - 1]);
            }

            // Tests if file record is parsed and end tags match
            FileRecord fileRecord = null;

            Assert.Throws<InvalidFileRecordException>(() =>
            {
                fileRecord = new FileRecord(fileRecordWithUsa, Volume);
            });

            Assert.Null(fileRecord);
        }
    }
}
