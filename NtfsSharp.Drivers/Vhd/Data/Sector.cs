namespace NtfsSharp.Drivers.Vhd.Data
{
    public class Sector
    {
        /// <summary>
        /// Number of bytes in a sector
        /// </summary>
        /// <remarks>According to the VHD specification, it's always 512 bytes</remarks>
        public const uint BytesPerSector = 512;

        public byte[] Data { get; }

        public static Sector Null
        {
            get
            {
                return new Sector(new byte[512]);
            }
        }

        public Sector(byte[] bytes)
        {
            Data = bytes;
        }
    }
}
