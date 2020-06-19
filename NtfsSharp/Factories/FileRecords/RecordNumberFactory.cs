using NtfsSharp.Facades;
using NtfsSharp.FileRecords;
using NtfsSharp.Volumes;

namespace NtfsSharp.Factories.FileRecords
{
    public static class RecordNumberFactory
    {
        /// <summary>
        /// Builds file record with specified record number and fixes it up
        /// </summary>
        /// <param name="recordNumber">File record number</param>
        /// <param name="owner">Where file record is located</param>
        /// <returns><seealso cref="FileRecord"/> object</returns>
        public static FileRecord Build(ulong recordNumber, Volume owner)
        {
            var bytesPerFileRecord = owner.SectorsPerMftRecord * owner.BytesPerSector;
            var offsetOfLcn = owner.MftLcn * owner.BytesPerSector * owner.SectorsPerCluster;

            owner.Driver.MoveFromBeginning((long) (offsetOfLcn + recordNumber * recordNumber));
            var data = owner.Driver.ReadSectorBytes(bytesPerFileRecord);

            return FileRecordFacade.Build(data, owner);
        }
    }
}
