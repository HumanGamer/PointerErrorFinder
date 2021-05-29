using System;
using System.Collections.Generic;
using System.IO;

namespace PointerErrorFinder
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("-- Pointer Error Finder --");

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: <inputfile.txt> [outputfile.txt]");
                return;
            }

            var inputFile = args[0];
            var outputFile = Path.ChangeExtension(inputFile, null) + "_output.txt";
            if (args.Length > 1)
                outputFile = args[1];

            Console.WriteLine("Parsing: " + inputFile);
            var outputErrors = ProcessFile(File.ReadAllText(inputFile));

            var outputLines = new List<string>();
            foreach (var err in outputErrors)
                outputLines.Add(err.ToString());
            
            Console.WriteLine("Writing Output: " + outputFile);
            File.WriteAllLines(outputFile, outputLines);
            
            Console.WriteLine("Done!");
        }

        private static List<PointerError> ProcessFile(string input)
        {
            var output = new List<PointerError>();
            
            var lines = input.Replace("\r\n", "\n").Split('\n');

            foreach (var line in lines)
            {
                if (line.Contains("*' to ") || line.Contains("*' of greater size"))
                    output.Add(ProcessLine(line));
            }
            
            return output;
        }

        private static PointerError ProcessLine(string line)
        {
            // TODO: Make configurable
            string filter = "/engine/source/";
            
            string file;
            if (string.IsNullOrEmpty(filter))
                file = line.Replace('\\', '/').Substring(0, "(");
            else
                file = line.Replace('\\', '/').Substring(0, filter, "(");

            int lineNum = line.Substring(0, "(", ")").ToInt32();

            ErrorType type = ErrorType.Unknown;
            if (line.Contains("*' to "))
                type = ErrorType.FromPointer;
            else if (line.Contains("*' of greater size"))
                type = ErrorType.ToPointer;

            string fromName = line.Substring(line.IndexOf(")"), " from '", "'");
            string toName = line.Substring(line.IndexOf(")"), "' to '", "'");

            return new PointerError()
            {
                Type = type,
                File = file,
                Line = lineNum,
                From = fromName,
                To = toName
            };
        }

        private static string Substring(this string self, int startIndex, string stopString)
        {
            int endIndex = self.IndexOf(stopString, startIndex);
            if (endIndex == -1)
                return null;

            return self.Substring(startIndex, endIndex - startIndex);
        }
        
        private static string Substring(this string self, int startIndex, string startString, string stopString)
        {
            startIndex = self.IndexOf(startString, startIndex);
            if (startIndex == -1)
                return null;

            startIndex += startString.Length;
            
            return self.Substring(startIndex, stopString);
        }

        private static string Align(this string self, int length)
        {
            string copy = self;
            
            while (copy.Length < length)
            {
                copy += ' ';
            }

            return copy;
        }

        private static int ToInt32(this string self)
        {
            return Convert.ToInt32(self);
        }

        private enum ErrorType
        {
            ToPointer,
            FromPointer,
            Unknown
        }

        private struct PointerError
        {
            public ErrorType Type { get; set; }
            public string File { get; set; }
            public int Line { get; set; }
            public string From { get; set; }
            public string To { get; set; }

            public override string ToString()
            {
                string fileLine = $"{File}({Line}):".Align(50);
                string type = $"[{Type}]".Align(13);
                string from = $"'{From}'".Align(23);
                return $"{fileLine} {type} {from} to '{To}'";
            }
        }
    }
}