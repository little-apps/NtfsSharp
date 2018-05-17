using NtfsSharp.Exceptions;
using NtfsSharp.Helpers;
using NtfsSharp.MetaData;

namespace NtfsSharp.Factories.MetaData
{
    public static class BootSectorFactory
    {
        /// <summary>
        /// Creates a BootSector object
        /// </summary>
        /// <param name="data">Data containing boot sector</param>
        /// <returns><seealso cref="BootSector"/> object</returns>
        /// <exception cref="InvalidBootSectorException">Thrown if BytesPerSector, ClustersPerMFTRecord or SectorsPerCluster is invalid.</exception>
        public static BootSector Build(byte[] data)
        {
            var bootSectorStructure = data.ToStructure<BootSector.NtfsBootSector>();

            var bytesPerSector = bootSectorStructure.BytesPerSector;
            var sectorsPerCluster = bootSectorStructure.SectorsPerCluster;

            if (bytesPerSector == 0)
                throw new InvalidBootSectorException(nameof(bootSectorStructure.BytesPerSector), "BytesPerSector cannot be zero.");

            if (bytesPerSector % 512 != 0)
                throw new InvalidBootSectorException(nameof(bootSectorStructure.BytesPerSector), "BytesPerSector must be multiple of 512.");

            if (bytesPerSector > 4096)
                throw new InvalidBootSectorException(nameof(bootSectorStructure.BytesPerSector), "BytesPerSector must be equal to or less than 4096.");

            if (sectorsPerCluster == 0)
                throw new InvalidBootSectorException(nameof(bootSectorStructure.SectorsPerCluster), "SectorsPerCluster cannot be zero.");

            uint bytesPerFileRecord;

            // If ClustersPerMFTRecord is positive (up to 0x7F), it represents clusters per MFT record
            if (bootSectorStructure.ClustersPerMFTRecord <= 0x7F)
                bytesPerFileRecord = 
                    (uint) (bootSectorStructure.ClustersPerMFTRecord * bootSectorStructure.BytesPerSector *
                            bootSectorStructure.SectorsPerCluster);
            else
            {
                // Otherwise if it's negative (from 0x80 to 0xFF), the size is 2 raised to its absolute value

                // Anything between 0x80 and 0xE0 will result in an integer overflow (since it's a 32 bit integer)
                if (bootSectorStructure.ClustersPerMFTRecord >= 0x80 &&
                    bootSectorStructure.ClustersPerMFTRecord <= 0xE0)
                    throw new InvalidBootSectorException(nameof(bootSectorStructure.ClustersPerMFTRecord),
                        "ClustersPerMFTRecord cannot be between 0xE0 and 0x80");

                bytesPerFileRecord = (uint) (1 << 256 - bootSectorStructure.ClustersPerMFTRecord);
            }
            
            return new BootSector(bootSectorStructure)
            {
                BytesPerFileRecord = bytesPerFileRecord,
                BytesPerSector = bytesPerSector,
                SectorsPerCluster = sectorsPerCluster
            };
        }
    }
}
