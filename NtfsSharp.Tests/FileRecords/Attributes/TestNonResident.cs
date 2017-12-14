﻿using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.Tests.Driver.Attributes.NonResident;
using NUnit.Framework;
using DataAttribute = NtfsSharp.FileRecords.Attributes.DataAttribute;

namespace NtfsSharp.Tests.FileRecords.Attributes
{
    [TestFixture]
    internal class TestNonResident : TestFileRecordBase
    {
        /// <summary>
        /// Test a positive LCN offset in data run
        /// </summary>
        [Test]
        public void TestPositiveLcn()
        {
            const ulong expectedFirstLcn = 100;
            const ulong expectedSecondLcn = 110;

            var nonResident = new NonResidentTestAttribute();

            var firstDataCluster = new byte[4096];

            firstDataCluster[0] = 0xde;
            firstDataCluster[firstDataCluster.Length - 1] = 0xad;

            var secondDataCluster = new byte[4096];

            secondDataCluster[0] = 0xbe;
            secondDataCluster[secondDataCluster.Length - 1] = 0xef;

            // Add first LCN at 100
            nonResident.AddDataAsVirtualClusters(firstDataCluster, expectedFirstLcn);

            // Add second LCN at -10 (relative to the first LCN at 100, which absolute is 90)
            nonResident.AddDataAsVirtualClusters(secondDataCluster, expectedSecondLcn);

            DummyFileRecord.Attributes.Add(nonResident);

            // Read dummy file record back
            var actualFileRecord = ReadDummyFileRecord();

            // Get the data attribute we just created
            var actualDataAttribute =
                (DataAttribute)actualFileRecord.FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA);

            // Make sure data attribute isn't null
            Assert.NotNull(actualDataAttribute);

            // Get non-resident header
            var actualNonResidentHeader = actualDataAttribute.Header as NtfsSharp.FileRecords.Attributes.Base.NonResident.NonResident;

            // Make sure it is non-resident header
            Assert.NotNull(actualNonResidentHeader);

            // Check that first VCN has LCN 100
            Assert.AreEqual(expectedFirstLcn, actualNonResidentHeader.VcnToLcn(0));

            // Check that second VCN has LCN 90
            Assert.AreEqual(expectedSecondLcn, actualNonResidentHeader.VcnToLcn(1));

            // Check data integrity
            var actualData = actualNonResidentHeader.GetAllDataAsBytes();

            Assert.AreEqual(0xde, actualData[0]);
            Assert.AreEqual(0xad, actualData[4095]);

            Assert.AreEqual(0xbe, actualData[4096]);
            Assert.AreEqual(0xef, actualData[8191]);
        }

        /// <summary>
        /// Test a negative LCN offset in data run
        /// </summary>
        [Test]
        public void TestNegativateLcn()
        {
            const ulong expectedFirstLcn = 100;
            const ulong expectedSecondLcn = 90;

            var nonResident = new NonResidentTestAttribute();

            var firstDataCluster = new byte[4096];

            firstDataCluster[0] = 0xde;
            firstDataCluster[firstDataCluster.Length - 1] = 0xad;
            
            var secondDataCluster = new byte[4096];

            secondDataCluster[0] = 0xbe;
            secondDataCluster[secondDataCluster.Length - 1] = 0xef;

            // Add first LCN at 100
            nonResident.AddDataAsVirtualClusters(firstDataCluster, expectedFirstLcn);

            // Add second LCN at -10 (relative to the first LCN at 100, which absolute is 90)
            nonResident.AddDataAsVirtualClusters(secondDataCluster, expectedSecondLcn);

            DummyFileRecord.Attributes.Add(nonResident);

            // Read dummy file record back
            var actualFileRecord = ReadDummyFileRecord();

            // Get the data attribute we just created
            var actualDataAttribute =
                (DataAttribute) actualFileRecord.FindAttributeBodyByType(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA);

            // Make sure data attribute isn't null
            Assert.NotNull(actualDataAttribute);

            // Get non-resident header
            var actualNonResidentHeader = actualDataAttribute.Header as NtfsSharp.FileRecords.Attributes.Base.NonResident.NonResident;

            // Make sure it is non-resident header
            Assert.NotNull(actualNonResidentHeader);

            // Check that first VCN has LCN 100
            Assert.AreEqual(expectedFirstLcn, actualNonResidentHeader.VcnToLcn(0));

            // Check that second VCN has LCN 90
            Assert.AreEqual(expectedSecondLcn, actualNonResidentHeader.VcnToLcn(1));

            // Check data integrity
            var actualData = actualNonResidentHeader.GetAllDataAsBytes();

            Assert.AreEqual(0xde, actualData[0]);
            Assert.AreEqual(0xad, actualData[4095]);

            Assert.AreEqual(0xbe, actualData[4096]);
            Assert.AreEqual(0xef, actualData[8191]);
        }
        
    }

    public class NonResidentTestAttribute : NonResidentAttributeBase
    {
        public NonResidentTestAttribute()
        {
            Header.Type = NTFS_ATTR_TYPE.DATA;
        }
    }
}
