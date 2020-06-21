using System;
using System.Collections;
using System.Collections.Generic;
using NtfsSharp.Exceptions;
using NtfsSharp.Facades;
using NtfsSharp.FileRecords;

namespace NtfsSharp.Volumes
{
    public class MasterFileTable : IReadOnlyDictionary<uint, FileRecord>
    {
        private const uint RecordsToRead = 26;

        private readonly uint _sectorsPerMftRecord;
        
        public readonly Volume Volume;

        private readonly SortedList<uint, FileRecord> _table = new SortedList<uint, FileRecord>();

        /// <summary>
        /// Constructor of MasterFileTable
        /// </summary>
        /// <param name="volume">Volume instance</param>
        /// <remarks>This object will have 0 elements until <seealso cref="Read"/> is called.</remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="volume"/> is null.</exception>
        public MasterFileTable(Volume volume)
        {
            Volume = volume ?? throw new ArgumentNullException(nameof(volume));

            _sectorsPerMftRecord = volume.BytesPerFileRecord / volume.BytesPerSector;
        }

        /// <summary>
        /// Reads master file table records from the LCN specified in <paramref name="mftLcn"/>.
        /// </summary>
        /// <param name="mftLcn">LCN of start of MFT.</param>
        /// <exception cref="InvalidMasterFileTableException">Thrown when the MFT record number does not match the index of it</exception>
        /// <remarks>
        ///     The attributes of each MFT record are parsed as well.
        /// </remarks>
        /// <returns>Current instance of <seealso cref="MasterFileTable"/></returns>
        public MasterFileTable Read(ulong mftLcn)
        {
            // Clear any existing file records in case a read is being performed after a bad read.
            if (_table.Count > 0)
                _table.Clear();

            var currentCluster = Volume.ReadLcn(mftLcn);
            var bytesPerFileRecord = _sectorsPerMftRecord * Volume.BytesPerSector;

            for (uint i = 0; i < RecordsToRead * _sectorsPerMftRecord; i += _sectorsPerMftRecord)
            {
                var sectorOffsetInLcn = i % Volume.SectorsPerCluster;

                if (sectorOffsetInLcn == 0 && i > 0)
                    currentCluster = Volume.ReadLcn(currentCluster.Lcn + 1);

                var fileRecordBytes = new byte[bytesPerFileRecord];

                Array.Copy(currentCluster.Data, sectorOffsetInLcn * Volume.BytesPerSector, fileRecordBytes, 0,
                    bytesPerFileRecord);

                try
                {
                    var fileRecord = FileRecordAttributesFacade.Build(fileRecordBytes, Volume);

                    var index = i / _sectorsPerMftRecord;
                    var recordNum = fileRecord.Header.MFTRecordNumber;
                    if (recordNum == 0)
                        recordNum = index;

                    if (recordNum != index)
                        throw new InvalidMasterFileTableException(nameof(fileRecord.Header.MFTRecordNumber),
                            "MFT Record Number must be 0 or match it's index in the MFT.", fileRecord);

                    _table.Add(recordNum, fileRecord);
                }
                catch (InvalidFileRecordException)
                {
                    // Some MFT system files may be empty
                    throw;
                }
                
            }

            return this;
        }

        #region IReadOnlyDictionary Implementation
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
        #endregion

        /// <summary>
        /// Represents the index of each NTFS file in the MFT
        /// </summary>
        public enum Files
        {
            /// <summary>
            /// Contains one base file record for each file and folder on an NTFS volume. If the allocation information for a file or folder is too large to fit within a single record, other file records are allocated as well.
            /// </summary>
            Mft = 0,
            /// <summary>
            /// Guarantees access to the MFT in case of a single-sector failure. It is a duplicate image of the first four records of the MFT.
            /// </summary>
            MftMirr = 1,
            /// <summary>
            /// Contains information used by NTFS for faster recoverability. The log file is used by Windows Server 2003 to restore metadata consistency to NTFS after a system failure. The size of the log file depends on the size of the volume, but you can increase the size of the log file by using the Chkdsk command.
            /// </summary>
            LogFile = 2,
            /// <summary>
            /// Contains information about the volume, such as the volume label and the volume version.
            /// </summary>
            Volume = 3,
            /// <summary>
            /// Lists attribute names, numbers, and descriptions.
            /// </summary>
            AttrDef = 4,
            /// <summary>
            /// The root folder.
            /// </summary>
            RootDir = 5,
            /// <summary>
            /// Represents the volume by showing free and unused clusters.
            /// </summary>
            Bitmap = 6,
            /// <summary>
            /// Includes the BPB used to mount the volume and additional bootstrap loader code used if the volume is bootable.
            /// </summary>
            Boot = 7,
            /// <summary>
            /// Contains bad clusters for a volume.
            /// </summary>
            BadClus = 8,
            /// <summary>
            /// Contains unique security descriptors for all files within a volume.
            /// </summary>
            Secure = 9,
            /// <summary>
            /// Converts lowercase characters to matching Unicode uppercase characters.
            /// </summary>
            Upcase = 10,
            /// <summary>
            /// Used for various optional extensions such as quotas, reparse point data, and object identifiers.
            /// </summary>
            Extend = 11
        }
    }
}
