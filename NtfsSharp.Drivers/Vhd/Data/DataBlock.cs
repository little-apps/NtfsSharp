using System;
using System.Collections;
using System.IO;
using NtfsSharp.Drivers.Vhd.ImageTypes;

namespace NtfsSharp.Drivers.Vhd.Data
{
    public class DataBlock
    {
        private readonly uint _fileLocation;
        private readonly DynamicImage _dynamicImage;

        public BitArray Bitmap { get; }
        public byte[] Data { get; }

        public DataBlock(uint fileLocation, DynamicImage dynamicImage)
        {
            _fileLocation = fileLocation;
            _dynamicImage = dynamicImage;

            // Each bit in the bitmap represents a sector so the number of bytes is the number of sectors * 8
            var bytesInBitmap = dynamicImage.SectorsPerBlock / 8;

            dynamicImage.Vhd.Stream.Seek(_fileLocation, SeekOrigin.Begin);
            
            var bitmapBytes = new byte[bytesInBitmap];
            dynamicImage.Vhd.Stream.Read(bitmapBytes, 0, bitmapBytes.Length);

            Bitmap = new BitArray(bitmapBytes);
        }

        public DataBlock(byte[] dataBlockBytes, DynamicImage dynamicImage)
        {
            _dynamicImage = dynamicImage;

            // Each bit in the bitmap represents a sector so the number of bytes is the number of sectors * 8
            var bytesInBitmap = dynamicImage.SectorsPerBlock / 8;

            var bitmapBytes = new byte[bytesInBitmap];

            Array.Copy(dataBlockBytes, 0, bitmapBytes, 0, bytesInBitmap);

            Data = new byte[dataBlockBytes.Length - bytesInBitmap];

            Array.Copy(dataBlockBytes, bytesInBitmap, Data, 0, Data.Length);
        }

        public Sector ReadSector(uint index)
        {
            if (index > Bitmap.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var sectorBytes = new byte[Sector.BytesPerSector];

            if (!Bitmap.Get((int) index))
                return Sector.Null;

            if (Data != null)
            {
                Array.Copy(Data, index * 512, sectorBytes, 0, sectorBytes.Length);
            }
            else
            {
                // Go to data block location + 512 (for bitmap) + index * 512
                _dynamicImage.Vhd.Stream.Seek(_fileLocation + (Bitmap.Length / 8) + (index * Sector.BytesPerSector),
                    SeekOrigin.Begin);

                _dynamicImage.Vhd.Stream.Read(sectorBytes, 0, sectorBytes.Length);
            }
            

            return new Sector(sectorBytes);
        }
    }
}
