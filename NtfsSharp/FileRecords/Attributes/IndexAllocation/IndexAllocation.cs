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
        public IndexAllocation(AttributeHeader header) : base(header, MustBe.NonResident)
        {
            
        }

        /// <summary>
        /// Reads file indices in IndexAllocation
        /// </summary>
        /// <remarks>This can a bit of time (depending on the size of the IndexAllocation)</remarks>
        /// <returns>List of file indices</returns>
        public IEnumerable<FileIndex> ReadFileIndices()
        {
            if (!Header.FileRecord.Header.Flags.HasFlag(FileRecord.Flags.IsDirectory))
                yield break;

            var headerNonResident = Header as NonResident;

            for (ulong i = 0; i <= headerNonResident.SubHeader.LastVCN; i++)
            {
                var lcn = headerNonResident.VcnToLcn(i);

                if (lcn == null)
                    continue;

                var cluster = headerNonResident.FileRecord.Volume.ReadLcn(lcn.Value);

                var magicNum = BitConverter.ToUInt32(cluster.Data, 0);

                if (magicNum != 0x58444E49)
                    break;

                yield return new FileIndex(cluster.Data);
            }
        }
    }
}
