using CommandLine;
using CommandLine.Text;

namespace NtfsSharp.Console
{
    class Options
    {
        [Option('o', "output", HelpText = "File to output to. If omitted, it is outputted to console")]
        public string OutputFile { get; set; }

        [OptionArray('d', "physical-drive", DefaultValue = new [] { "" }, HelpText = "Physical drive and partition index to open. If not specified, 'partition' is used.")]
        public string[] PhysicalDrive { get; set; }

        [Option('p', "partition", DefaultValue = 'C', HelpText = "Partition drive letter to open.")]
        public char Drive { get; set; }

        [Option("list-drives", HelpText = "Lists the physical drives")]
        public bool ListPhysicalDrives { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("NtfsSharp", "1.0"),
                Copyright = new CopyrightInfo("Little Apps", 2017),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            help.AddPreOptionsLine("Usage: app [DriveLetter]");
            help.AddOptions(this);
            return help;
        }
    }
}
