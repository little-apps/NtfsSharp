NtfsSharp
=====================
NtfsSharp parses a NTFS (or New Technology File System) volume using C#

## Development ##

NtfsSharp is currently a work in progress and therefore, it is not working 100%. If you would like to help develop it, please [contact me](http://www.little-apps.com/contact/).

## License ##
NtfsSharp is free and open source, and is licensed under the MIT License. 

## Notes ##
 * This library uses P/Invoke functions from the Windows API so it doesn't work out of the box with other operating systems
 * Currently, this library is purely for reading a NTFS volume and not writing to it
 * The ``FILE_FLAG_BACKUP_SEMANTICS`` flag is not used when opening a handle to the volume. Therefore, the ``SE_BACKUP_NAME`` and ``SE_RESTORE_NAME`` are not required from the calling process.

## Credits ##
 * [CommandLineParser by Giacomo Stelluti Scala](https://github.com/gsscoder/commandline)
