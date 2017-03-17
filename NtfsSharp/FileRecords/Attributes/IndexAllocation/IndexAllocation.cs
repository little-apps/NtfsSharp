using System;
using System.Collections.Generic;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.Base.NonResident;

namespace NtfsSharp.FileRecords.Attributes.IndexAllocation
{
    /// <summary>
    /// Represents $INDEX_ALLOCATION attribute
    /// </summary>
    /// <remarks>
    /// Must be located in non-resident data
    /// </remarks>
    public class IndexAllocation : AttributeBodyBase
    {
        public readonly List<FileIndex> FileIndices = new List<FileIndex>();

        /// <summary>
        /// Finds file indices in index
        /// </summary>
        /// <param name="header"></param>
        public IndexAllocation(AttributeHeader header) : base(header, MustBe.NonResident)
        {
            if (!header.FileRecord.Header.Flags.HasFlag(FileRecord.Flags.IsDirectory))
                return;

            var headerNonResident = header as NonResident;

            for (ulong i = 0; i <= headerNonResident.SubHeader.LastVCN; i++)
            {
                var lcn = headerNonResident.VcnToLcn(i);

                if (lcn == null)
                    continue;

                var cluster = headerNonResident.FileRecord.Volume.ReadLcn(lcn.Value);

                var magicNum = BitConverter.ToUInt32(cluster.Data, 0);

                if (magicNum != 0x58444E49)
                    break;

                FileIndices.Add(new FileIndex(cluster.Data));
            }
        }
    }
}
