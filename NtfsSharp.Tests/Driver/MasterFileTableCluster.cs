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
        public readonly DummyFileRecord[] FileRecords;

        public bool UseUpdateSequenceArray { get; set; } = false;

        public ushort EndTag { get; set; } = 0;
        public ushort[] FixUps { get; set; } = {0, 0};

        /// <inheritdoc />
        /// <remarks>An array of bytes set to zero are added if the index in <seealso cref="FileRecords"/> is null.</remarks>
        protected override bool ShouldGenerateDefault => false;

        public MasterFileTableCluster(DummyDriver driver, uint filesPerPart, uint bytesPerFileRecord, uint lcn)
        {
            _driver = driver;
            FilesPerPart = filesPerPart;
            BytesPerFileRecord = bytesPerFileRecord;
            Lcn = lcn;

            FileRecords = new DummyFileRecord[filesPerPart];
        }
        
        protected override void GenerateDefaultDummy()
        {
            
        }

        public override byte[] Build()
        {
            var bytes = new byte[FilesPerPart * BytesPerFileRecord];

            for (var i = 0; i < FileRecords.Length; i++)
            {
                var fileRecord = FileRecords[i];

                byte[] fileRecordBytes;

                if (fileRecord != null)
                    fileRecordBytes =
                        UseUpdateSequenceArray
                            ? fileRecord.BuildWithUsa(BytesPerFileRecord, _driver, EndTag, FixUps)
                            : fileRecord.Build(BytesPerFileRecord, _driver);
                else
                    fileRecordBytes = new byte[BytesPerFileRecord];

                Array.Copy(fileRecordBytes, 0, bytes, i * BytesPerFileRecord, BytesPerFileRecord);
            }

            return bytes;
        }
    }
}
