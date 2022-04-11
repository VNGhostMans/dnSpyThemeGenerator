using System;
using System.IO;
using CommandLine;
using dnSpyThemeGenerator.Converters;
using dnSpyThemeGenerator.Themes;

namespace dnSpyThemeGenerator
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Usage:");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("     dnSpyThemeGenerator -i \"JetBrainsTheme.xml\" -d \"dnSpy\\bin\\Themes\\dark.dntheme\" -o \"OutPutThemeFile.dntheme\"");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("-i   (input)  :  The input theme to convert");
                Console.WriteLine("-d   (donor)  :  The dnSpy theme to use as base");
                Console.WriteLine("-o   (output) :  The output path to write the new dnSpy theme to");
                Console.ReadKey();
                return;
            }    
            Parser.Default.ParseArguments<CommandLineArguments>(args).WithParsed(RunOptions);
        }

        private static void RunOptions(CommandLineArguments args)
        {
            var input = RiderTheme.ReadFromStream(File.OpenRead(args.ThemePath));
            var donor = DnSpyTheme.ReadFromStream(File.OpenRead(args.DonorPath));
            new RiderToDnSpyConverter().CopyTo(input, donor);
            donor.WriteToStream(File.Open(args.OutputPath, FileMode.Create));
        }
    }
}