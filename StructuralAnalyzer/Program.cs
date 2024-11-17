using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace StructuralAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var parser = new Parser();
                var visitor = new StructuralVisitor();
                var files = Directory.GetFiles(args[0], "*.cs", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var tree = parser.Parse(file);
                    visitor.Visit(tree.GetRoot());
                }

                var callMatrix = visitor.GetCallMatrix();
                DisplayCallMatrix(callMatrix);

                var louvain = new LouvainAlgorithm(callMatrix);
                var louvainCommunities = louvain.Execute();
                DisplayCommunities("Louvain Communities", louvainCommunities);

                var leiden = new LeidenAlgorithm(callMatrix);
                var leidenCommunities = leiden.Execute();
                DisplayCommunities("Leiden Communities", leidenCommunities);

                SuggestMicroservices(louvainCommunities, "Louvain");
                SuggestMicroservices(leidenCommunities, "Leiden");

                var outputPath = "output/microservices";
                Directory.CreateDirectory(outputPath);

                var microserviceGenerator = new MicroserviceGenerator(outputPath);
                microserviceGenerator.GenerateMicroservices(louvainCommunities, callMatrix);
                microserviceGenerator.GenerateMicroservices(leidenCommunities, callMatrix);

                DisplayMicroserviceInterfaces(outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        static void DisplayCallMatrix(Dictionary<string, Dictionary<string, Dictionary<string, int>>> callMatrix)
        {
            Console.WriteLine("\n||>>>>>>>>>>>>>>>>>>>>>>\n");
            Console.WriteLine($"Call Matrix:\n");
            foreach (var classEntry in callMatrix)
            {
                Console.WriteLine($"Class: {classEntry.Key}");
                foreach (var methodEntry in classEntry.Value)
                {
                    Console.WriteLine($"  Method: {methodEntry.Key}");
                    foreach (var referenceEntry in methodEntry.Value)
                    {
                        Console.WriteLine($"    Calls: {referenceEntry.Key} - Count: {referenceEntry.Value}");
                    }
                }
            }
            Console.WriteLine("\n<<<<<<<<<<<<<<<<<<<<<<||\n\n");
        }

        static void DisplayCommunities(string title, Dictionary<string, string> communities)
        {
            Console.WriteLine($"\n||>>>>>>>>>>>>>>>>>>>>>>\n");
            Console.WriteLine($"{title}:\n");
            foreach (var community in communities)
            {
                Console.WriteLine($"Vertex: {community.Key} - Community: {community.Value}");
            }
            Console.WriteLine("\n<<<<<<<<<<<<<<<<<<<<<<||\n\n");
        }

        static void SuggestMicroservices(Dictionary<string, string> communities, string algorithm)
        {
            Console.WriteLine($"\n||>>>>>>>>>>>>>>>>>>>>>>\n");
            Console.WriteLine($"Sugestões de Microserviços ({algorithm}):\n");

            var groupedCommunities = communities.GroupBy(c => c.Value).ToDictionary(g => g.Key, g => g.Select(c => c.Key).ToList());

            foreach (var community in groupedCommunities)
            {
                Console.WriteLine($"Microserviço: {community.Key}");
                foreach (var vertex in community.Value)
                {
                    Console.WriteLine($"  Método: {vertex}");
                }
            }

            Console.WriteLine("\n<<<<<<<<<<<<<<<<<<<<<<||\n\n");
        }

        static void DisplayMicroserviceInterfaces(string outputPath)
        {
            var directories = Directory.GetDirectories(outputPath);
            foreach (var directory in directories)
            {
                var interfaceFiles = Directory.GetFiles(directory, "I*.cs");
                foreach (var interfaceFile in interfaceFiles)
                {
                    Console.WriteLine($"\n||>>>>>>>>>>>>>>>>>>>>>>\n");
                    Console.WriteLine($"Interface: {Path.GetFileName(interfaceFile)}\n");
                    var code = File.ReadAllText(interfaceFile);
                    Console.WriteLine(code);
                    Console.WriteLine("\n<<<<<<<<<<<<<<<<<<<<<<||\n\n");
                }
            }
        }
    }
}
