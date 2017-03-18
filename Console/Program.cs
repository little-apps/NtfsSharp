using System.IO;
using NtfsSharp;

namespace Console
{
    internal class Program
    {
        private NtfsSharp.Volume Volume;
        private Options Options;

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

            Volume = new NtfsSharp.Volume(Options.Drive);

            Interactive();
        }

        private void Interactive()
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

                    default:
                        break;
                }
            }
            
        }

        private void DisplayCommands(TextWriter textWriter)
        {
            textWriter.WriteLine("Available commands:");
            textWriter.WriteLine("1\t\tDisplay boot sector");
            textWriter.WriteLine("2\t\tList MFT");
            textWriter.WriteLine("Q\t\tQuit");
            textWriter.WriteLine();
            textWriter.Write("Enter command: ");
        }

        private void OutputBootSectorInfo(TextWriter textWriter)
        {
            textWriter.WriteLine("JMP Instruction: {0}", Volume.BootSector.JMPInstruction.MakeReadable());
            textWriter.WriteLine("OEMID: {0}", Volume.BootSector.OEMID.MakeReadable());
            textWriter.WriteLine("Bytes Per Sector: {0}", Volume.BootSector.BytesPerSector);
            textWriter.WriteLine("Sectors Per Cluster: {0}", Volume.BootSector.SectorsPerCluster);
            textWriter.WriteLine("Reserved Sectors: {0}", Volume.BootSector.ReservedSectors);
            textWriter.WriteLine("Media Descriptor: {0}", Volume.BootSector.MediaDescriptor);
            textWriter.WriteLine("Sectors Per Track: {0}", Volume.BootSector.SectorsPerTrack);
            textWriter.WriteLine("Number Of Heads: {0}", Volume.BootSector.NumberOfHeads);
            textWriter.WriteLine("Hidden Sectors: {0}", Volume.BootSector.HiddenSectors);
            textWriter.WriteLine("Total Sectors: {0}", Volume.BootSector.TotalSectors);
            textWriter.WriteLine("MFT LCN: {0}", Volume.BootSector.MFTLCN);
            textWriter.WriteLine("MFT Mirror LCN: {0}", Volume.BootSector.MFTMirrLCN);
            textWriter.WriteLine("Clusters Per MFT Record: {0}", Volume.BootSector.ClustersPerMFTRecord);
            textWriter.WriteLine("Clusters Per Index Buffer: {0}", Volume.BootSector.ClustersPerIndexBuffer);
            textWriter.WriteLine("Volume Serial Number: {0:X}", Volume.BootSector.VolumeSerialNumber);
            textWriter.WriteLine("NTFS Checksum: {0:X}", Volume.BootSector.NTFSChecksum);

            textWriter.WriteLine("Signature: {0}", Volume.BootSector.Signature.MakeReadable());
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

        static void Main(string[] args)
        {
            var options = new Options();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                var program = new Program(options);
            }
        }
    }
}
