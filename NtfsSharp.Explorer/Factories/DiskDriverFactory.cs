using System;
using NtfsSharp.Drivers;

namespace NtfsSharp.Explorer.Factories
{
    internal static class DiskDriverFactory
    {
        /// <summary>
        /// Makes a <seealso cref="IDiskDriver"/> based on specified <seealso cref="Options"/>
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Instance of <seealso cref="IDiskDriver"/></returns>
        /// <exception cref="Exception">Thrown if unknown options selected.</exception>
        internal static IDiskDriver Make(Options options)
        {
            switch (options.MediaType)
            {
                case Options.MediaTypes.Drive:
                    return new PartitionDriver($@"\\.\{options.SelectedDriveLetter}:");

                case Options.MediaTypes.VhdFile: 
                    return new VhdDriver(options.VhdFile);

                default:
                    throw new Exception("Unknown media type selected.");
            }
        }
    }
}
