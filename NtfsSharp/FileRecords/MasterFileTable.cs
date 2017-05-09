using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NtfsSharp.FileRecords
{
    public class MasterFileTable : IReadOnlyDictionary<uint, FileRecord>
    {
        private const uint RecordsToRead = 26;

        private readonly uint _sectorsPerMftRecord;
        private readonly Volume _volume;
        private readonly SortedList<uint, FileRecord> _table = new SortedList<uint, FileRecord>();

        public MasterFileTable(Volume volume)
        {
            _volume = volume;

            _sectorsPerMftRecord = volume.BytesPerFileRecord / volume.BytesPerSector;
        }

        public void ReadRecords()
        {
            var currentOffset = _volume.LcnToOffset(_volume.BootSector.MFTLCN);

            for (uint i = 0; i < RecordsToRead; i++)
            {
                var bytes = new byte[_sectorsPerMftRecord * _volume.BytesPerSector];

                for (var j = 0; j < _sectorsPerMftRecord; j++)
                {
                    var sector = _volume.ReadSectorAtOffset(currentOffset);

                    Array.Copy(sector.Data, 0, bytes, j * _volume.BytesPerSector, _volume.BytesPerSector);

                    currentOffset += _volume.BytesPerSector;
                }

                var fileRecord = new FileRecord(bytes, _volume);
                fileRecord.ReadAttributes();

                var recordNum = fileRecord.Header.MFTRecordNumber;
                if (recordNum == 0)
                    recordNum = i;

                _table.Add(recordNum, fileRecord);
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
