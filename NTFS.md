# NTFS
# Flow #

 1. Read boot sector
 2. Read MFT (Master File Table)
 3. Read file records
 4. Get attributes for each file record 

# Boot Sector #
 * Offset: 0
 * Size: 512 bytes
 * Calculate cluster size by multiplying bytes per sector by sectors per cluster

# Master File Table #
 * Offset: ``[BootSector.LogicalClusterMFT] * [BootSector.ClusterSize]``
 * Size: 1024 bytes
 * Contains following file records:
   1.  

# File Record #
 * Offset: ``[BootSectorSize] + [MFTCount] * [MFTEntrySize]`` (root directory)
 * Size: 1024 bytes (as of NT 5.0)
 * ``NTFS_ATTRIBUTE_HEADER.Length`` is the whole size of the attribute, including the data

## $FILE_NAME (0x30) ##
 * ``FileRecordNumber`` refers to the parent directory of file record
 * If ``FileRecordNumber`` is 5, it is in the root directory

## $DATA (0x80) ##
 * Can be resident or non-resident
 * If resident, data contains contents of file
 * If non-resident, 

## Resident vs Non-Resident ##
 * A resident attribute is when the attribute can fit within the 1024 bytes of the file records
 * A non-resident attribute is when the attribute does not fit in the file record itself and is placed in one or more clusters elsewhere in the volume.
 * The filename and time stamp are always resident attributes.

## Terminology ##

### Sectors ###
 * Sectors are the smallest physical storage unit on the hard drive
 * Usually 512 bytes long

### Clusters ###
 * Consists of one or more sectors
 * The number of sectors in a cluster must be a exponent of 2
 * Could be 1, 2, 4, 8, 16, etc sectors long
 * Usually 8 sectors long

### Logical Cluster Number (LCN) ###
 * Each LCN is given a sequential number
 * LCN 0 (zero) refers first cluster on the hard drive (the boot sector)

### Virtual Cluster Number (VCN) ###
 * Any cluster in a file has a virtual cluster number (VCN), which is its relative offset from the beginning of the file.
 * Each cluster of a non-resident stream is given a sequential number.
 * To locate the stream on disk, it's necessary to convert from a VCN to an LCN. This is done with the help of data runs. 
 * VCN 0 (zero) refers to the first cluster of the stream.
 * For example, a seek to twice the size of a cluster, followed by a read, will return data beginning at the third VCN.
 
## References ##
 * https://0cch.com/ntfsdoc/index.html