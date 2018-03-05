namespace NtfsSharp.Drivers.Physical
{
    public class Partition
    {
        /// <summary>
        /// The starting sector of the selected partition
        /// </summary>
        public ulong StartSector { get; }

        /// <summary>
        /// The last sector (from the start of the drive) of the selected partition
        /// </summary>
        public ulong EndSector { get; }

        public MasterBootRecord Mbr { get; }

        public Partition(ulong startSector, ulong endSector, MasterBootRecord masterBootRecord)
        {
            StartSector = startSector;
            EndSector = endSector;
            Mbr = masterBootRecord;
        }
    }
}
