using CommandLine;
using CommandLine.Text;

namespace Console
{
    class Options
    {
        [Option('o', "output", HelpText = "File to output to. If omitted, it is outputted to console")]
        public string OutputFile { get; set; }

        [ValueOption(0)]
        public char Drive { get; set; } = 'C';

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
