using System;
using System.IO;

namespace NtfsSharp.Explorer
{
    internal class Options
    {
        internal MediaTypes MediaType { get; set; } = MediaTypes.Drive;

        internal DriveInfo[] AvailableDrives { get; }

        internal uint SelectedDriveIndex { get; set; }

        internal char SelectedDriveLetter
        {
            get { return AvailableDrives[SelectedDriveIndex].Name[0]; }
        }

        internal string VhdFile { get; set; }

        internal Options()
        {
            AvailableDrives = DriveInfo.GetDrives();
        }

        internal void IsValid()
        {
            switch (MediaType)
            {
                case MediaTypes.Drive:
                {
                    if (SelectedDriveIndex > AvailableDrives.Length)
                        throw new Exception("Unknown drive selected.");

                    break;
                }

                case MediaTypes.VhdFile:
                {
                    if (string.IsNullOrWhiteSpace(VhdFile))
                        throw new Exception("VHD file path must be specified.");

                    if (!File.Exists(VhdFile))
                        throw new Exception("VHD file could not be found.");

                    break;
                }

                default:
                {
                    throw new Exception("Unknown media type selected.");
                    break;
                }
            }
        }

        internal enum MediaTypes
        {
            Drive,
            VhdFile
        }
    }
}
