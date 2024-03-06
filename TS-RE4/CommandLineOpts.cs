using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSRE4.CommandLineOpts
{
    [Verb("extract", HelpText = "Extract file or files from a pak")]
    public class PAKExtract
    {
        [Option('i', "input", Required = true, HelpText = "The source PAK to extract")]
        public string InputPak { get; set; }

        [Option('o', "output", Required = true, HelpText = "Where to extract the files")]
        public string DestDir { get; set; }
    }
}
