namespace NtfsSharp.Contracts
{
    public interface IDiskDriver
    {
        /// <summary>
        /// Guessed number of sectors in a cluster.
        /// </summary>
        uint DefaultSectorsPerCluster { get; }

        /// <summary>
        /// Guessed number of bytes in a sector.
        /// </summary>
        ushort DefaultBytesPerSector { get; }

        /// <summary>
        /// Moves to a position inside the NTFS file system from the beginning.
        /// </summary>
        /// <param name="offset">Offset in the NTFS to move to from beginning.</param>
        /// <returns>New offset after move.</returns>
        long MoveFromBeginning(long offset);

        /// <summary>
        /// Moves to a position inside the NTFS file system from the current position.
        /// </summary>
        /// <param name="offset">Offset in the NTFS to move to.</param>
        /// <returns>New offset after move.</returns>
        long MoveFromCurrent(long offset);

        /// <summary>
        /// Moves to a position inside the NTFS file system from the end.
        /// </summary>
        /// <param name="offset">Offset in the NTFS to move to from the end.</param>
        /// <returns>New offset after move.</returns>
        long MoveFromEnd(long offset);

        /// <summary>
        /// Reads the number of bytes from inside the NTFS
        /// </summary>
        /// <param name="bytesToRead">Number of bytes to read</param>
        /// <remarks>
        /// Expect the <paramref name="bytesToRead"/> to be a multiple of the NTFS sector size (usually 512 bytes).
        /// The position the read (set by <seealso cref="MoveFromBeginning"/>) occurs at the start of a sector in the NTFS
        /// </remarks>
        /// <returns>Bytes read as array</returns>
        byte[] ReadSectorBytes(uint bytesToRead);

        /// <summary>
        /// Allows for bytes to be read outside of the bounds of the sector size
        /// </summary>
        /// <param name="bytesToRead">Number of bytes to read</param>
        /// <remarks>
        /// Unlike <seealso cref="ReadSectorBytes"/>, the position and <paramref name="bytesToRead"/> may not be a multiple of the sector size. 
        /// </remarks>
        /// <returns>Bytes read as array</returns>
        byte[] ReadInsideSectorBytes(uint bytesToRead);
    }
}
