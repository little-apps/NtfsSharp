﻿using System.Linq;
using System.Text;
using NtfsSharp.Exceptions;
using NtfsSharp.Facades;
using NtfsSharp.Files;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace NtfsSharp.Tests.FileRecords
{
    [TestFixture]
    public class TestFileRecord : TestFileRecordBase
    {
        
        /// <summary>
        /// Tests the MAGIC identifier is valid
        /// </summary>
        [Test]
        public void TestFileRecordMagicValid()
        {
            var expectedMagic = Encoding.ASCII.GetBytes("FILE");

            DummyFileRecord.FileRecord.Magic = expectedMagic;

            FileRecord fileRecord = null;

            Assert.DoesNotThrow(() =>
                fileRecord = FileRecordFacade.Build(DummyFileRecord.BuildWithUsa(BytesPerFileRecord, Driver, 0xab),
                    Volume));

            ClassicAssert.IsNotNull(fileRecord);
            ClassicAssert.AreEqual(expectedMagic, fileRecord.Header.Magic);
        }
        
        /// <summary>
        /// Tests the MAGIC identifier is not valid
        /// </summary>
        [Test]
        public void TestFileRecordMagicIdInvalid()
        {
            DummyFileRecord.FileRecord.Magic = new byte[] {0, 0, 0, 0};

            var ex =
                Assert.Throws<InvalidFileRecordException>(() =>
                    FileRecordFacade.Build(DummyFileRecord.BuildWithUsa(BytesPerFileRecord, Driver, 0xab), Volume));
            
            ClassicAssert.AreEqual(nameof(DummyFileRecord.FileRecord.Magic), ex.ParamName);
        }

        /// <summary>
        /// Test file record containing all zeroes is invalid
        /// </summary>
        [Test]
        public void TestFileRecordZeroes()
        {
            var fileRecordBytes = new byte[BytesPerFileRecord];

            ClassicAssert.IsFalse(fileRecordBytes.Any(b => b != 0));

            var ex = Assert.Throws<InvalidFileRecordException>(() => FileRecordFacade.Build(fileRecordBytes, Volume));

            // Should fail at magic identifier
            ClassicAssert.AreEqual(nameof(DummyFileRecord.FileRecord.Magic), ex.ParamName);
        }
    }
}
