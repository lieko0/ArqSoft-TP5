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
                    visitor.Visits(tree, tree.GetRoot());
                }

                var callMatrix = visitor.GetCallMatrix();
                Console.WriteLine("Graph:\nNumber of vertex: " + callMatrix.Count);
                // DisplayCallMatrix(callMatrix);
                
                var louvain = new LouvainAlgorithm(callMatrix);
                var louvainCommunities = louvain.Execute();
                DisplayCommunities("Louvain Communities", louvainCommunities, callMatrix); 
                
                /*
                // Run the louvain again on the results of the first run
                var louvain2 = new LouvainAlgorithm(callMatrix, louvainCommunities);
                var louvainCommunities2 = louvain2.Execute();
                DisplayCommunities("Louvain Communities 2", louvainCommunities2, callMatrix);

                DisplayCommunitiesByAggregation("Louvain Communities Aggregated 2", louvainCommunities2, callMatrix);
                */
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
                foreach (var classReceiver in classEntry.Value)
                {
                    Console.WriteLine($"  Receiver: {classReceiver.Key}");
                    foreach (var referenceEntry in classReceiver.Value)
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
            var communityGroups = communities.GroupBy(c => c.Value);
            foreach (var community in communityGroups)
            {
                if(community.Count() > 1)
                {
                    Console.WriteLine($"Community: {community.Key}");
                    foreach (var vertex in community)
                    {
                        Console.WriteLine($"  {vertex.Key}");
                    }

                }
            }
            Console.WriteLine("\n<<<<<<<<<<<<<<<<<<<<<<||\n\n");

        }


    }
}




