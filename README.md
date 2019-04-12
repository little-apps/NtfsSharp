NtfsSharp
=====================
[![Build Status](https://travis-ci.org/little-apps/NtfsSharp.svg?branch=master)](https://travis-ci.org/little-apps/NtfsSharp) [![Build status](https://ci.appveyor.com/api/projects/status/ny7ro7468l64xulv?svg=true)](https://ci.appveyor.com/project/little-apps/ntfssharp) [![Coverage Status](https://coveralls.io/repos/github/little-apps/NtfsSharp/badge.svg?branch=master)](https://coveralls.io/github/little-apps/NtfsSharp?branch=master)


NtfsSharp parses a NTFS (or New Technology File System) volume using C#

## Development ##

NtfsSharp is currently a work in progress and therefore, it is not working 100%. If you would like to help develop it, please [contact me](http://www.little-apps.com/contact/).

## License ##
NtfsSharp is free and open source, and is licensed under the MIT License. 

 > Copyright 2019 Little Apps
 > 
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
