using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NtfsSharp.FileRecords
{
    public class MasterFileTable : IReadOnlyDictionary<uint, FileRecord>
    {
        private const uint _recordsToRead = 26;

        private readonly uint SectorsPerMFTRecord;
        private readonly Volume Volume;
        private readonly SortedList<uint, FileRecord> _table = new SortedList<uint, FileRecord>();

        public MasterFileTable(Volume volume)
        {
            Volume = volume;

            SectorsPerMFTRecord = volume.BytesPerFileRecord / volume.BytesPerSector;

            var currentOffset = volume.LcnToOffset(Volume.BootSector.MFTLCN);

            for (uint i = 0; i < _recordsToRead; i++)
            {
                var bytes = new byte[SectorsPerMFTRecord * volume.BytesPerSector];

                for (var j = 0; j < SectorsPerMFTRecord; j++)
                {
                    var sector = volume.ReadSectorAtOffset(currentOffset);

                    Array.Copy(sector.Data, 0, bytes, j * volume.BytesPerSector, volume.BytesPerSector);

                    currentOffset += volume.BytesPerSector;
                }

                var fileRecord = new FileRecord(bytes, volume);
                fileRecord.ReadAttributes();

                _table.Add(fileRecord.Header.MFTRecordNumber, fileRecord);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<uint, FileRecord>> GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        public int Count => _table.Count;

        public bool ContainsKey(uint key)
        {
            return _table.ContainsKey(key);
        }

        public bool TryGetValue(uint key, out FileRecord value)
        {
            return _table.TryGetValue(key, out value);
        }

        public FileRecord this[uint key] => _table[key];

        public IEnumerable<uint> Keys => _table.Keys;
        public IEnumerable<FileRecord> Values => _table.Values;
    }
}
