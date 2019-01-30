using System;
using System.Diagnostics;
using System.IO;

namespace Kv
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Key-Value Parser Library");
                Console.WriteLine();
                Console.WriteLine("Arguments:");
                Console.WriteLine("\t0\tDisplay this help");
                Console.WriteLine("\t1\tRead file from 1st argument and output its data to standard output.");
                Console.WriteLine("\t2\tRead file from 1st argument and output its data to file from 2nd argument.");
                return 0;
            }

            string inputFile = args[0];
            if (string.IsNullOrEmpty(inputFile))
            {
                Console.Error.WriteLine("Invalid input file name.");
                return 1;
            }
            if (!File.Exists(inputFile))
            {
                Console.Error.WriteLine("Input file does not exist.");
                return 1;
            }
            
            Stopwatch stopwatch_load = new Stopwatch();
            
            stopwatch_load.Start();
            var kv = KvFile.Load(inputFile);
            stopwatch_load.Stop();

            if (args.Length == 1)
            { // Standard Output
                kv.Save(Console.Out);

                return 0;
            }
            else
            { // Output file
                Console.WriteLine("Load Time: "+stopwatch_load.Elapsed.TotalMilliseconds+" ms");
                
                string outputFile = args[1];
                if (string.IsNullOrEmpty(outputFile))
                {
                    Console.Error.WriteLine("Invalid output file name.");
                    return 1;
                }
                
                Stopwatch stopwatch_save = new Stopwatch();
                
                stopwatch_save.Start();
                kv.Save(outputFile);
                stopwatch_save.Stop();
                Console.WriteLine("Save Time: "+stopwatch_save.Elapsed.TotalMilliseconds+" ms");
                Console.WriteLine("Total Time: "+(stopwatch_load.Elapsed + stopwatch_save.Elapsed).TotalMilliseconds+" ms");

                return 0;
            }
        }
    }
}