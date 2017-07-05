namespace FileNormalizer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using Appccelerate.CommandLineParser;
    using Appccelerate.IO.Access;

    public class Program
    {
        public static void Main(string[] args)
        {
            string inputFolder = null;
            string filter = "*";
            string targetEncoding = null;
            string outputFolder = null;
            bool convertToWindows = false;
            var configuration = CommandLineParserConfigurator
                .Create()
                    .WithNamed("inputFolder", a => inputFolder = a)
                    .Required()
                    .WithNamed("filter", a => filter = a)
                    .WithNamed("outputFolder", a => outputFolder = a)
                    .WithSwitch("convertToWindowsNewLines", () => convertToWindows = true)
                    .WithNamed("targetEncoding", a => targetEncoding = a)
                    .Required()
                .BuildConfiguration();

            var commandLineParser = new CommandLineParser(configuration);
            commandLineParser.Parse(args);

            var accessFactory = new AccessFactory();
            IDirectory directory = accessFactory.CreateDirectory();
            IEnumerable<string> inputFiles = directory.GetFiles(inputFolder, filter, SearchOption.AllDirectories);

            Encoding encoding = Encoding.GetEncoding(targetEncoding);

            foreach (var inputFilePath in inputFiles)
            {
                var fileContent = accessFactory.CreateFile().ReadAllText(inputFilePath);

                string outputPath = inputFilePath;
                if (!string.IsNullOrEmpty(outputFolder))
                {
                    outputPath = outputPath.Replace(inputFolder, outputFolder);

                    IDirectory targetDirectory = accessFactory.CreateDirectory();
                    if (!targetDirectory.Exists(outputPath))
                    {
                        IDirectoryInfo parent = targetDirectory.GetParent(outputPath);
                        targetDirectory.CreateDirectory(parent.FullName);
                    }
                }

                if (convertToWindows)
                {
                    fileContent = Regex.Replace(fileContent, @"\r\n?|\n", "\r\n");
                }

                accessFactory.CreateFile().WriteAllText(outputPath, fileContent, encoding);
            }

            Console.ReadKey();
        }
    }
}
