using CommandLine;
using Serilog;
using System.Reflection.Metadata.Ecma335;
using TSRE4.Pak;
using TSRE4.CommandLineOpts;

namespace TSRE4
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            return CommandLine.Parser.Default.ParseArguments<PAKExtract>(args)
                .MapResult(
                  (PAKExtract opts) => RunPAKExtract(opts),
                  errs => 1);
        }

        private static int RunPAKExtract(PAKExtract opts)
        {
            Log.Information("Extracting files from {pakPath} to {destPath}", opts.InputPak, opts.DestDir);

            if (!File.Exists(opts.InputPak))
            {
                Log.Error("Can't find pak {pakPath}", opts.InputPak);
                return 1;
            }

            var mainPak = new PCK2();
            mainPak.Load(opts.InputPak);
            mainPak.ExtractAllFiles(opts.DestDir);

            return 0;
        }
    }
}
