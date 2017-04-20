using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NtfsSharp;
using NtfsSharp.FileRecords.Attributes.Base;
using NtfsSharp.FileRecords.Attributes.Base.NonResident;

namespace DiskUsage
{
    public class UsageCollection : Collection<Usage>
    {
        private const uint BitmapRecordNum = 6;

        private Volume _volume;
        private uint BytesPerBit => _volume.BytesPerSector * _volume.SectorsPerCluster;

        public readonly Usage Used = new Usage("Used");
        public readonly Usage Free = new Usage("Free");

        public UsageCollection()
        {
            Add(Used);
            Add(Free);
        }

        public async Task UpdateVolume(Volume volume)
        {
            _volume = volume;

            await Task.Run((Action) UpdateUsage);
        }

        private void UpdateUsage()
        {
            var bitmapFile = _volume.MFT[BitmapRecordNum];

            var dataAttr = bitmapFile.FindAttributeByType(AttributeHeaderBase.NTFS_ATTR_TYPE.DATA);

            if (dataAttr == null)
                throw new FileNotFoundException("Could not locate DATA attribute in $Bitmap file");

            byte[] bitmapBytes;

            if (!dataAttr.Header.Header.NonResident)
            {
                bitmapBytes = dataAttr.Body.Body;
            }
            else
            {
                var nonResidentAttr = dataAttr.Header as NonResident;
                bitmapBytes = nonResidentAttr.GetAllDataAsBytes();
            }

            var bitArray = new BitArray(bitmapBytes);

            var totalBytes = bitArray.Length * BytesPerBit;

            Used.Value = bitArray.Cast<bool>().Count(bit => bit) * BytesPerBit;
            Free.Value = totalBytes - Used.Value;
            
        }
    }
}
