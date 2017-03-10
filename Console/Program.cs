using System.IO;

namespace Console
{
    internal class Program
    {
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

            var ntfs = new NtfsSharp.Volume(Options.Drive);

            ntfs.DisplayInfo(Output);
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
