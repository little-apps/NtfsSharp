using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLine;
using NtfsSharp.Console.Exceptions;
using NtfsSharp.Drivers;
using NtfsSharp.Drivers.Physical;
using NtfsSharp.Volumes;

namespace NtfsSharp.Console
{
    internal class Program
    {
        private Volume Volume;
        private Options Options;

        private uint? DriveNum { get; set; }
        private uint? PartitionNum { get; set; }

        private TextWriter Output
        {
            get
            {
                if (string.IsNullOrEmpty(Options.OutputFile))
                    return System.Console.Out;

                return File.CreateText(Options.OutputFile);
            }
        }

        private Program(Options options)
        {
            Options = options;

            ValidateOptions();

            if (options.ListPhysicalDrives)
            {
                ListPhysicalDrives();
                return;
            }

            try
            {
                if (DriveNum.HasValue && PartitionNum.HasValue)
                    Volume = new Volume(new PhysicalDiskDriver($@"\\.\PhysicalDrive{DriveNum}", PartitionNum.Value));
                else
                    Volume = new Volume(new PartitionDriver($@"\\.\{Options.Drive}:"));
            }
            catch (Exception ex)
            {
                throw new InvalidOptionsException(ex.Message, ex);
            }

            Volume.Read();
        }

        private void ListPhysicalDrives()
        {
            var i = 0;

            Output.WriteLine("Available drives:");
            Output.WriteLine();
            
            foreach (var physicalDrive in PhysicalDiskDriver.GetPhysicalDrives())
            {
                Output.WriteLine("{0}: {1}", i, physicalDrive);
                i++;
            }
        }

        private int InteractiveMode()
        {
            var cmd = ' ';

            while (cmd != 'Q')
            {
                DisplayCommands(System.Console.Out);

                cmd = (char) System.Console.Read();

                switch (cmd)
                {
                    case '1':
                    {
                        OutputBootSectorInfo(Output);
                        break;
                    }

                    case '2':
                    {
                        ListMFT(Output);
                        break;
                    }

                    case '3':
                    {
                        ReadFileRecords(false, Output);
                        break;
                    }


                    case '4':
                    {
                        ReadFileRecords(true, Output);
                        break;
                    }

                    default:
                        break;
                }
            }

            return 0;
        }

        private void DisplayCommands(TextWriter textWriter)
        {
            textWriter.WriteLine("Available commands:");
            textWriter.WriteLine("1\t\tDisplay boot sector");
            textWriter.WriteLine("2\t\tList MFT");
            textWriter.WriteLine("3\t\tRead file records (without attributes)");
            textWriter.WriteLine("4\t\tRead file records (with attributes)");
            textWriter.WriteLine("Q\t\tQuit");
            textWriter.WriteLine();
            textWriter.Write("Enter command: ");
        }

        private void OutputBootSectorInfo(TextWriter textWriter)
        {
            textWriter.WriteLine("JMP Instruction: {0}", Volume.BootSector.BootSectorStructure.JMPInstruction.MakeReadable());
            textWriter.WriteLine("OEMID: {0}", Volume.BootSector.BootSectorStructure.OEMID.MakeReadable());
            textWriter.WriteLine("Bytes Per Sector: {0}", Volume.BootSector.BytesPerSector);
            textWriter.WriteLine("Sectors Per Cluster: {0}", Volume.BootSector.SectorsPerCluster);
            textWriter.WriteLine("Reserved Sectors: {0}", Volume.BootSector.BootSectorStructure.ReservedSectors);
            textWriter.WriteLine("Media Descriptor: {0}", Volume.BootSector.BootSectorStructure.MediaDescriptor);
            textWriter.WriteLine("Sectors Per Track: {0}", Volume.BootSector.BootSectorStructure.SectorsPerTrack);
            textWriter.WriteLine("Number Of Heads: {0}", Volume.BootSector.BootSectorStructure.NumberOfHeads);
            textWriter.WriteLine("Hidden Sectors: {0}", Volume.BootSector.BootSectorStructure.HiddenSectors);
            textWriter.WriteLine("Total Sectors: {0}", Volume.BootSector.BootSectorStructure.TotalSectors);
            textWriter.WriteLine("MFT LCN: {0}", Volume.BootSector.BootSectorStructure.MFTLCN);
            textWriter.WriteLine("MFT Mirror LCN: {0}", Volume.BootSector.BootSectorStructure.MFTMirrLCN);
            textWriter.WriteLine("Clusters Per MFT Record: {0}", Volume.BootSector.BootSectorStructure.ClustersPerMFTRecord);
            textWriter.WriteLine("Clusters Per Index Buffer: {0}", Volume.BootSector.BootSectorStructure.ClustersPerIndexBuffer);
            textWriter.WriteLine("Volume Serial Number: {0:X}", Volume.BootSector.BootSectorStructure.VolumeSerialNumber);
            textWriter.WriteLine("NTFS Checksum: {0:X}", Volume.BootSector.BootSectorStructure.NTFSChecksum);

            textWriter.WriteLine("Signature: {0}", Volume.BootSector.BootSectorStructure.Signature.MakeReadable());
        }

        private void ListMFT(TextWriter textWriter)
        {
            if (Volume.MFT.Count == 0)
            {
                textWriter.WriteLine("Nothing in Master File Table.");
                return;
            }

            foreach (var kvp in Volume.MFT)
            {
                textWriter.WriteLine("{0}: {1}", kvp.Key, kvp.Value.Filename);
            }
        }

        private void ReadFileRecords(bool readAttrs, TextWriter textWriter)
        {
            var stopWatch = Stopwatch.StartNew();

            using (var progress = new ProgressBar())
            {
                var i = 0;
                var totalInodes = Volume.TotalInodes;

                foreach (var fileRecord in Volume.ReadFileRecords(readAttrs))
                {
                    progress.Report((double)i / totalInodes);

                    i++;
                }
            }

            
        }

        private void ValidateOptions()
        {
            if (Options.PhysicalDrive.Any())
            {
                if (Options.PhysicalDrive.Count() != 2)
                {
                    throw new InvalidOptionsException("Two options (drive and partition number) are not specified.");
                }

                if (string.IsNullOrEmpty(Options.PhysicalDrive.ElementAt(0)) || string.IsNullOrEmpty(Options.PhysicalDrive.ElementAt(1)))
                {
                    throw new InvalidOptionsException("Either the drive or partition number are empty.");
                }

                try
                {
                    DriveNum = Convert.ToUInt16(Options.PhysicalDrive.ElementAt(0));
                }
                catch (Exception ex)
                {
                    throw new InvalidOptionsException("Drive must a number between 0-65535", ex);
                }

                try
                {
                    PartitionNum = Convert.ToUInt16(Options.PhysicalDrive.ElementAt(1));
                }
                catch (Exception ex)
                {
                    throw new InvalidOptionsException("Partition must a number between 0-65535", ex);
                }
            }
            else
            {
                if (!char.IsUpper(Options.Drive))
                {
                    throw new InvalidOptionsException("Drive must be a upper case letter (A-Z)");
                }
            }
            
        }

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Console.Options>(args).MapResult(options =>
            {
                try
                {
                    var program = new Program(options);
                    return program.InteractiveMode();
                }
                catch (InvalidOptionsException ex)
                {
                    System.Console.WriteLine(ex.Message);
                    return 1;
                }
               
            }, _ => 1);
        }
    }
}
