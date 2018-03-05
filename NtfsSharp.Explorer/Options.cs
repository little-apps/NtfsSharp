using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtfsSharp.Explorer
{
    internal class Options
    {
        private uint _selectedDrive;
        private string _vhdFile;

        internal MediaTypes MediaType { get; set; } = MediaTypes.Drive;

        internal DriveInfo[] AvailableDrives { get; }

        internal uint SelectedDriveIndex
        {
            get => _selectedDrive;
            set => _selectedDrive = value;
        }

        internal char SelectedDriveLetter
        {
            get { return AvailableDrives[SelectedDriveIndex].Name[0]; }
        }

        internal string VhdFile
        {
            get => _vhdFile;
            set => _vhdFile = value;
        }

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
                    if (_selectedDrive > AvailableDrives.Length)
                        throw new Exception("Unknown drive selected.");

                    break;
                }

                case MediaTypes.VhdFile:
                {
                    if (string.IsNullOrWhiteSpace(_vhdFile))
                        throw new Exception("VHD file path must be specified.");

                    if (!File.Exists(_vhdFile))
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
