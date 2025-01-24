NtfsSharp
=====================
[![.NET](https://github.com/SameOldNick/NtfsSharp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/SameOldNick/NtfsSharp/actions/workflows/dotnet.yml) [![Build status](https://ci.appveyor.com/api/projects/status/4vdeypbb8dv4faxj?svg=true)](https://ci.appveyor.com/project/SameOldNick/ntfssharp) [![Coverage Status](https://coveralls.io/repos/github/SameOldNick/NtfsSharp/badge.svg?branch=master)](https://coveralls.io/github/SameOldNick/NtfsSharp?branch=master)

NtfsSharp is a C# library designed to parse and analyze NTFS (New Technology File System) volumes.

## Development ##

NtfsSharp is currently a work in progress and therefore, it is not working 100%. If you would like to help develop it, please [contact me](https://www.sameoldnick.com/contact).

## License ##
NtfsSharp is free and open source, and is licensed under the MIT License. 

 > Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 > 
 > The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 > 
 > THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Notes ##
 * This library uses P/Invoke functions from the Windows API so it doesn't work out of the box with other operating systems
 * Currently, this library is purely for reading a NTFS volume and not writing to it
 * The ``FILE_FLAG_BACKUP_SEMANTICS`` flag is not used when opening a handle to the volume. Therefore, the ``SE_BACKUP_NAME`` and ``SE_RESTORE_NAME`` privileges are not required from the calling process.

## Credits ##
 * [CommandLineParser by Giacomo Stelluti Scala](https://github.com/gsscoder/commandline)
 * [ProgressBar in Console C# by co89757](https://gist.github.com/co89757/5ae15bf61a62f82f9abd32a285f0c76a)
