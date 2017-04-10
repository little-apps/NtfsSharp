using System;
using System.IO;
using NtfsSharp.Data;

namespace NtfsSharp.FileRecords.Attributes.Base.NonResident
{
    /// <summary>
    /// Used to read file contents directly from hard disk
    /// </summary>
    /// <remarks>
    /// Unlike most built-in streams in C#, this is not a double buffered stream
    /// </remarks>
    public class DataStream : Stream
    {
        public byte[] ResidentData { get; private set; }
        public NonResident NonResidentAttribute { get; private set; }
        public Volume Volume => NonResidentAttribute.FileRecord.Volume;

        public override bool CanRead { get; } = true;
        public override bool CanSeek { get; } = true;
        public override bool CanWrite { get; } = false;

        public override long Length => ResidentData?.LongLength ?? (long) NonResidentAttribute.SubHeader.AttributeSize;

        public override long Position { get; set; }

        public bool EndOfFile => Position == Length;

        private uint CurrentVcn { get; set; }
        private ulong CurrentLcn { get; set; }
        private uint OffsetInLcn { get; set; }

        private Cluster Cluster => Volume.ReadLcn(CurrentLcn);

        private uint ClusterSize
            =>
                NonResidentAttribute.FileRecord.Volume.BytesPerSector *
                NonResidentAttribute.FileRecord.Volume.SectorsPerCluster;

        private uint MaxVcn { get; set; }

        public DataStream(AttributeBase attribute)
        {
            if (!attribute.Header.Header.NonResident)
                ReadResidentData(attribute.Header as Resident);
            else
                ReadNonResidentData(attribute.Header as NonResident);
        }

        private void ReadResidentData(Resident attribute)
        {
            ResidentData = attribute.ReadBody();
        }

        private void ReadNonResidentData(NonResident attribute)
        {
            NonResidentAttribute = attribute;

            foreach (var dataBlock in NonResidentAttribute.DataBlocks)
            {
                if (!dataBlock.VirtualFragment && CurrentLcn == 0)
                {
                    var lcn = NonResidentAttribute.VcnToLcn(MaxVcn);

                    if (!lcn.HasValue)
                        throw new IOException("Unable to determine start LCN");

                    CurrentVcn = MaxVcn;
                    CurrentLcn = lcn.Value;
                }

                MaxVcn += dataBlock.RunLength;
            }
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long actualOffset = -1;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    actualOffset = offset;
                    break;
                case SeekOrigin.Current:
                    actualOffset = Position + offset;
                    break;
                case SeekOrigin.End:
                    actualOffset = Length + offset;
                    break;
            }

            if (actualOffset < 0 || actualOffset > Length)
                throw new ArgumentException("Trying to seek before beginning or after end of file.", nameof(offset));

            if (actualOffset == Position)
                // Nothing to do
                return Position;

            return ResidentData != null ? SeekResident(actualOffset) : SeekNonResident(actualOffset);
        }

        private long SeekResident(long actualOffset)
        {
            Position = actualOffset;

            return Position;
        }

        private long SeekNonResident(long actualOffset)
        {
            // What cluster do we need to be in?
            var vcn = (ulong)(actualOffset / ClusterSize);

            var currentLcn = NonResidentAttribute.VcnToLcn(vcn);

            if (!currentLcn.HasValue)
                throw new IOException("Unable to determine logical cluster number from virtual cluster number.");

            CurrentLcn = currentLcn.Value;
            OffsetInLcn = (uint)(actualOffset % ClusterSize);

            Position = actualOffset;
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("The length cannot be changed for this stream");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null.");

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");

            if (buffer.Length - offset < count)
                throw new ArgumentException("Buffer is too small", nameof(buffer));

            //if (count > Length)
            //    throw new ArgumentOutOfRangeException(nameof(count), "Count is larger than size of $DATA stream");

            if (EndOfFile)
                return 0;

            return ResidentData != null ? ReadResident(buffer, offset, count) : ReadNonResident(buffer, offset, count);
        }

        private int ReadResident(byte[] buffer, int offset, int count)
        {
            if (Position + count > Length)
                count = (int) (count - (Position + count - Length));

            Array.Copy(ResidentData, Position, buffer, offset, count);

            Position += count;

            return count;
        }

        /// <summary>
        /// Reads non-resident data into buffer
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        private int ReadNonResident(byte[] buffer, int offset, int count)
        {
            ulong currentLcn;
            long offsetInCluster;
            Cluster currentCluster;
            int totalBytesRead = 0;

            while (offset < count)
            {
                if (EndOfFile)
                    break;

                currentLcn = PositionToLcn(out offsetInCluster);
                currentCluster = Volume.ReadLcn(currentLcn);

                var bytesRead = ClusterSize - offsetInCluster;

                if (Position + ClusterSize > Length)
                    bytesRead = bytesRead - (Position + ClusterSize - Length);

                Array.Copy(currentCluster.Data, offsetInCluster, buffer, offset, bytesRead);

                totalBytesRead += (int) bytesRead;
                offset += (int) bytesRead;
                Position += bytesRead;
            }

            return totalBytesRead;


            //Array.Copy(currentCluster.Data, offsetInCluster, buffer, offset, ClusterSize - offsetInCluster);

            //Position += ClusterSize + offsetInCluster;

            // Get current LCN

            // # of bytes to read = ClusterSize - offsetInLcn

            // If offset + # bytes to read <= count

            //       Read from that cluster

            // Otherwise if Position + # bytes to read > count
            //       Read 

            // If the # of bytes read < count -> read next cluster
        }

        private ulong PositionToLcn(out long offsetInCluster)
        {
            var vcn = (ulong) (Position / ClusterSize);
            offsetInCluster = Position % ClusterSize;

            if (vcn >= MaxVcn)
                throw new EndOfStreamException("Trying to read past end of clusters");

            var nextLcn = NonResidentAttribute.VcnToLcn(vcn);

            if (!nextLcn.HasValue)
                throw new IOException("Unable to determine next logical cluster number.");

            return nextLcn.Value;
        }

        /// <summary>
        /// Reads non-resident data into buffer
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        private int ReadNonResident2(byte[] buffer, int offset, int count)
        {
            var endOfStream = false;
            var startPosition = Position;
            var totalBytesRead = 0;

            while (count - totalBytesRead > ClusterSize)
            {
                var bytesRead = ClusterSize - OffsetInLcn;
                
                Array.Copy(Cluster.Data, OffsetInLcn, buffer, offset, bytesRead);

                totalBytesRead += (int) bytesRead;
                offset += (int) bytesRead;
                Position += bytesRead;

                if (startPosition + totalBytesRead >= Length)
                {
                    totalBytesRead = (int) (totalBytesRead - (startPosition + totalBytesRead - Length));
                    Position = Length;
                    endOfStream = true;
                    break;
                }

                OffsetInLcn += ClusterSize - OffsetInLcn;

                if (OffsetInLcn >= ClusterSize)
                {
                    // Get offset in next cluster
                    OffsetInLcn = ClusterSize - bytesRead;

                    if (++CurrentVcn == MaxVcn)
                    {
                        // We're at the last VCN, which is the end of the stream
                        endOfStream = true;
                        break;
                    }

                    // Advance to next LCN
                    var nextLcn = NonResidentAttribute.VcnToLcn(CurrentVcn);

                    if (!nextLcn.HasValue)
                        throw new IOException("Unable to determine next logical cluster number.");

                    CurrentLcn = nextLcn.Value;
                }
            }

            var bufferLeft = count - totalBytesRead;

            if (count - totalBytesRead > 0 && !endOfStream)
            {
                Array.Copy(Cluster.Data, OffsetInLcn, buffer, offset, bufferLeft);
                totalBytesRead += bufferLeft;
            }

            //Position += totalBytesRead;

            //if (endOfStream)
            //{

            //    if (Position > Length)
            //        Position = Length;
            //}

            return totalBytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("This stream does not support writing.");
        }

        
    }
}
