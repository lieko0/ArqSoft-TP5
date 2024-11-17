using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

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
                Console.WriteLine("Graph:\nNumber of vertex: " + callMatrix.Count);
                //DisplayCallMatrix(callMatrix);

                var louvain = new LouvainAlgorithm(callMatrix);
                var louvainCommunities = louvain.Execute();
                DisplayCommunities("Louvain Communities", louvainCommunities, callMatrix);

                // Run the louvain again on the results of the first run
                var louvain2 = new LouvainAlgorithm(callMatrix, louvainCommunities);
                var louvainCommunities2 = louvain2.Execute();
                DisplayCommunities("Louvain Communities 2", louvainCommunities2, callMatrix);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        static void DisplayCallMatrix(Dictionary<string, Dictionary<string, Dictionary<string, int>>> callMatrix)
        {
            Console.WriteLine("\n||>>>>>>>>>>>>>>>>>>>>>>\n");
            Console.WriteLine($"Graph:\n");
            //number of vertex
            Console.WriteLine($"Number of Vertex: {callMatrix.Count}");
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

        static void DisplayCommunities(string title, Dictionary<string, string> communities, Dictionary<string, Dictionary<string, Dictionary<string, int>>> callMatrix)
        {
            Console.WriteLine($"\n||>>>>>>>>>>>>>>>>>>>>>>\n");
            Console.WriteLine($"{title}:\n");

            var groupedCommunities = communities.GroupBy(c => c.Value).ToDictionary(g => g.Key, g => g.Select(c => c.Key).ToList());
            Console.WriteLine($"Number of Communities: {groupedCommunities.Count}\n");
            int ignored = 0;
            foreach (var community in groupedCommunities)
            {
                
                var classesInCommunity = new HashSet<string>();

                foreach (var method in community.Value)
                {
                    foreach (var classEntry in callMatrix)
                    {
                        if (classEntry.Value.ContainsKey(method))
                        {
                            classesInCommunity.Add(classEntry.Key);
                        }
                    }
                }
                if (classesInCommunity.Count == 1)
                {
                    ignored++;
                    continue;
                }
                Console.WriteLine($"Community: {community.Key}");
                foreach (var className in classesInCommunity)
                {
                    Console.WriteLine($"  Class: {className}");
                }
            }

            Console.WriteLine($"\nWith more than one class: {groupedCommunities.Count - ignored} \nIgnored: {ignored}");

            Console.WriteLine("\n<<<<<<<<<<<<<<<<<<<<<<||\n\n");
        }

    }
}
