using System;
using NtfsSharp.Contracts;

namespace NtfsSharp.Drivers
{
    public abstract class BaseDiskDriver : IDiskDriver, IDisposable
    {
        /// <summary>
        /// Guessed sectors per cluster
        /// </summary>
        /// <remarks>Since we don't know the size of a cluster yet (cause that info is in the boot sector) to do a read, we can only guess that so we can read the bootsector.</remarks>
        public virtual uint DefaultSectorsPerCluster => 8;

        /// <summary>
        /// Guessed bytes per sector
        /// </summary>
        /// <remarks>Since we don't know the size of a sector yet (cause that info is in the boot sector) to do a read, we can only guess that so we can read the bootsector.</remarks>
        public virtual ushort DefaultBytesPerSector => 512;

        /// <summary>
        /// Moves to a position inside the NTFS file system
        /// </summary>
        /// <param name="offset">Offset in the NTFS to move to</param>
        /// <param name="moveMethod">Whether the <paramref name="offset"/> is from the beginning, current position or end of the NTFS</param>
        /// <returns></returns>
        public abstract long Move(long offset, MoveMethod moveMethod = MoveMethod.Begin);
        
        public long MoveFromBeginning(long offset)
        {
            return Move(offset, MoveMethod.Begin);
        }

        public long MoveFromCurrent(long offset)
        {
            return Move(offset, MoveMethod.Current);
        }

        public long MoveFromEnd(long offset)
        {
            return Move(offset, MoveMethod.End);
        }

        /// <summary>
        /// Reads the number of bytes from inside the NTFS
        /// </summary>
        /// <param name="bytesToRead">Number of bytes to read</param>
        /// <remarks>
        /// Expect the <paramref name="bytesToRead"/> to be a multiple of the NTFS sector size (usually 512 bytes).
        /// The position the read (set by <seealso cref="Move"/>) occurs at the start of a sector in the NTFS
        /// </remarks>
        /// <returns>Bytes read as array</returns>
        public abstract byte[] ReadSectorBytes(uint bytesToRead);

        /// <summary>
        /// Allows for bytes to be read outside of the bounds of the sector size
        /// </summary>
        /// <param name="bytesToRead">Number of bytes to read</param>
        /// <remarks>
        /// Unlike <seealso cref="ReadSectorBytes"/>, the position and <paramref name="bytesToRead"/> may not be a multiple of the sector size. 
        /// </remarks>
        /// <returns>Bytes read as array</returns>
        public abstract byte[] ReadInsideSectorBytes(uint bytesToRead);

        public enum MoveMethod : uint
        {
            Begin = 0,
            Current = 1,
            End = 2
        }

        public abstract void Dispose();
    }
}