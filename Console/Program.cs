using System.IO;

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
