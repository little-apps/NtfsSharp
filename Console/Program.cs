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
