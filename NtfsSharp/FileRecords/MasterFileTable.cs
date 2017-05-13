using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NtfsSharp.Exceptions;

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
            var currentCluster = _volume.ReadLcn(_volume.BootSector.MFTLCN);
            var bytesPerFileRecord = _sectorsPerMftRecord * _volume.BytesPerSector;
            
            for (uint i = 0; i < RecordsToRead * _sectorsPerMftRecord; i += _sectorsPerMftRecord)
            {
                var sectorOffsetInLcn = i % _volume.SectorsPerCluster;

                if (sectorOffsetInLcn == 0 && i > 0)
                    currentCluster = _volume.ReadLcn(currentCluster.Lcn + 1);

                var fileRecordBytes = new byte[bytesPerFileRecord];

                Array.Copy(currentCluster.Data, sectorOffsetInLcn * _volume.BytesPerSector, fileRecordBytes, 0,
                    bytesPerFileRecord);

                var fileRecord = new FileRecord(fileRecordBytes, _volume);
                fileRecord.ReadAttributes();

                var index = i / 2;
                var recordNum = fileRecord.Header.MFTRecordNumber;
                if (recordNum == 0)
                    recordNum = index;

                if (recordNum != index)
                    throw new InvalidMasterFileTableException(nameof(fileRecord.Header.MFTRecordNumber),
                        "MFT Record Number must be 0 or match it's index in the MFT.", fileRecord);

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
