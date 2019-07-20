using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace NtfsSharp.Console
{
    class Options
    {
        [Option('o', "output", HelpText = "File to output to. If omitted, it is outputted to console")]
        public string OutputFile { get; set; }

        [Option('d', "physical-drive", Separator = ':', HelpText = "Physical drive and partition index to open. If not specified, 'partition' is used.")]
        public IEnumerable<string> PhysicalDrive { get; set; }

        [Option('p', "partition", Default = 'C', HelpText = "Partition drive letter to open.")]
        public char Drive { get; set; }

        [Option("list-drives", HelpText = "Lists the physical drives")]
        public bool ListPhysicalDrives { get; set; }

        [Usage(ApplicationAlias = "NtfsSharp")]
        public static IEnumerable<Example> Examples =>
            new List<Example>() {
                new Example("Get information on drive C", new Options { Drive = 'C' }),
                new Example("Get information of first partition in first physical drive", new Options { PhysicalDrive = new[] { "0", "0" } })
            };
    }
}
