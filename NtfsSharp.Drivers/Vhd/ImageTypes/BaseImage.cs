using NtfsSharp.Drivers.Vhd.Data;

namespace NtfsSharp.Drivers.Vhd.ImageTypes
{
    public abstract class BaseImage
    {
        public Vhd Vhd { get; }

        public ulong TotalSectors { get; protected set; }

        protected BaseImage(Vhd vhd)
        {
            Vhd = vhd;
        }

        public abstract Sector ReadSector(uint sector);
    }
}
