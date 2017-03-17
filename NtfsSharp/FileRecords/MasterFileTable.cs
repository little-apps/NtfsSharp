using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NtfsSharp.FileRecords
{
    public class MasterFileTable
    {
        private const uint _recordsToRead = 16;

        private readonly uint SectorsPerMFTRecord;
        private readonly Volume Volume;
        public readonly ReadOnlyDictionary<uint, FileRecord> Table;

        public MasterFileTable(Volume volume)
        {
            Volume = volume;

            SectorsPerMFTRecord = volume.BytesPerFileRecord / volume.BytesPerSector;

            var fileRecords = new Dictionary<uint, FileRecord>((int) _recordsToRead);

            var currentOffset = volume.LcnToOffset(Volume.BootSector.MFTLCN);

            for (var i = 0; i < _recordsToRead; i++)
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

                fileRecords.Add(fileRecord.Header.MFTRecordNumber, fileRecord);
            }

            Table = new ReadOnlyDictionary<uint, FileRecord>(fileRecords);
        }
    }
}
