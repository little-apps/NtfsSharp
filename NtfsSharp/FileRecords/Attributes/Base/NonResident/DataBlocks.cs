using System;
using System.Collections;
using System.Collections.Generic;
using NtfsSharp.Exceptions;

namespace NtfsSharp.FileRecords.Attributes.Base.NonResident
{
    public class DataBlocks : IReadOnlyList<DataBlock>
    {
        private readonly List<DataBlock> _dataBlocks;

        public int Count
        {
            get => _dataBlocks.Count;
        }

        public DataBlock this[int index] => _dataBlocks[index];

        internal DataBlocks(List<DataBlock> dataBlocks)
        {
            _dataBlocks = dataBlocks ?? throw new ArgumentNullException(nameof(dataBlocks));
        }

        public bool Contains(DataBlock dataBlock)
        {
            return _dataBlocks.Contains(dataBlock);
        }

        public IEnumerator<DataBlock> GetEnumerator()
        {
            return _dataBlocks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Builds <see cref="DataBlocks"/> from data and <seealso cref="NonResident"/> header
        /// </summary>
        /// <param name="data">Data containing datablocks</param>
        /// <param name="nonResidentHeader">The non resident header the datablocks are for</param>
        /// <param name="currentOffset">The current offset in the <paramref name="data"/></param>
        /// <returns><see cref="DataBlocks"/> instance</returns>
        public static DataBlocks BuildFromData(byte[] data, NonResident nonResidentHeader, ref uint currentOffset)
        {
            ulong currentActualLcn = 0;
            ulong vcn = 0;
            var isFirst = true;
            var dataBlocks = new List<DataBlock>();

            while (currentOffset < nonResidentHeader.Header.Length && data[currentOffset] != 0)
            {
                var dataBlock = DataBlock.GetDataBlockFromRun(data, ref currentOffset, vcn);

                if (isFirst)
                {
                    if (dataBlock.LcnOffsetNegative)
                        throw new InvalidAttributeException("The first virtual cluster cannot have a negative LCN.");

                    isFirst = false;
                }

                currentActualLcn = dataBlock.ActualLcnOffset =
                    (ulong) ((long) currentActualLcn + dataBlock.SignedLcnOffset);

                if (dataBlock.LastVcn > nonResidentHeader.SubHeader.LastVCN - nonResidentHeader.SubHeader.StartingVCN)
                {
                    dataBlocks.Clear();
                    break;
                }

                dataBlocks.Add(dataBlock);

                vcn += dataBlock.RunLength;
            }

            return new DataBlocks(dataBlocks);
        }
    }
}
