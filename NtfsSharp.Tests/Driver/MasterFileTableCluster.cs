using System;
using System.Collections.Generic;

namespace NtfsSharp.Tests.Driver
{
    class MasterFileTableCluster : BaseDriverCluster
    {
        private readonly DummyDriver _driver;

        public readonly uint FilesPerPart;
        public readonly uint BytesPerFileRecord;
        public readonly uint Lcn;
        public readonly List<DummyFileRecord> FileRecords;

        public bool UseUpdateSequenceArray { get; set; } = false;

        public ushort EndTag { get; set; } = 0;
        public ushort[] FixUps { get; set; } = {0, 0};

        protected override bool ShouldGenerateDefault
        {
            get { return FileRecords.Count == 0; }
        }

        public MasterFileTableCluster(DummyDriver driver, uint filesPerPart, uint bytesPerFileRecord, uint lcn)
        {
            _driver = driver;
            FilesPerPart = filesPerPart;
            BytesPerFileRecord = bytesPerFileRecord;
            Lcn = lcn;

            FileRecords = new List<DummyFileRecord>((int) FilesPerPart);
        }
        
        protected override void GenerateDefaultDummy()
        {
            for (var i = 0; i < FilesPerPart; i++)
            {
                FileRecords.Add(DummyFileRecord.BuildDummyFileRecord(0));
            }
        }

        public override byte[] Build()
        {
            var bytes = new byte[FilesPerPart * BytesPerFileRecord];

            for (var i = 0; i < FileRecords.Count; i++)
            {
                var fileRecord = FileRecords[i];

                Array.Copy(
                    UseUpdateSequenceArray
                        ? fileRecord.BuildWithUsa(BytesPerFileRecord, _driver, EndTag, FixUps)
                        : fileRecord.Build(BytesPerFileRecord, _driver), 0, bytes,
                    i * BytesPerFileRecord, BytesPerFileRecord);
            }

            return bytes;
        }
    }
}
