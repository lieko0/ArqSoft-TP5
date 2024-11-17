using System;
using System.Collections.Generic;
using System.Linq;

namespace StructuralAnalyzer
{
    public class LeidenAlgorithm
    {
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, int>>> _callMatrix;

        public LeidenAlgorithm(Dictionary<string, Dictionary<string, Dictionary<string, int>>> callMatrix)
        {
            _callMatrix = callMatrix;
        }

        public Dictionary<string, string> Execute()
        {
            var graph = BuildGraph();
            var communities = InitializeCommunities(graph);
            bool modularityImproved;

            do
            {
                modularityImproved = false;

                // Phase 1: Local modularity optimization
                foreach (var vertex in graph.Keys.ToList())
                {
                    communities[vertex] = vertex; // Put vertex in its own community
                    var bestCommunity = communities[vertex];
                    var bestModularityGain = 0.0;

                    foreach (var neighbor in graph[vertex])
                    {
                        if (!communities.ContainsKey(neighbor)) continue;

                        var community = communities[neighbor];
                        var modularityGain = CalculateModularityGain(vertex, community, communities, graph);

                        if (modularityGain > bestModularityGain)
                        {
                            bestCommunity = community;
                            bestModularityGain = modularityGain;
                            break; // Move vertex to community of neighbor if modularity gain
                        }
                    }

                    if (bestCommunity != communities[vertex])
                    {
                        communities[vertex] = bestCommunity;
                        modularityImproved = true;
                    }
                }

                // Phase 2: Community aggregation
                if (modularityImproved)
                {
                    graph = AggregateCommunities(graph, communities);
                    communities = InitializeCommunities(graph);
                }

                // Phase 2: Partition refinement
                foreach (var community in communities.Values.Distinct().ToList())
                {
                    var partition = communities.Where(c => c.Value == community).Select(c => c.Key).ToList();
                    foreach (var vertex in partition)
                    {
                        if (partition.Count == 1)
                        {
                            communities[vertex] = vertex; // Assign vertex to new community using probability distribution P
                        }
                    }
                }

            } while (modularityImproved);

            return communities;
        }

        private Dictionary<string, List<string>> BuildGraph()
        {
            var graph = new Dictionary<string, List<string>>();

            foreach (var classEntry in _callMatrix)
            {
                foreach (var methodEntry in classEntry.Value)
                {
                    foreach (var referenceEntry in methodEntry.Value)
                    {
                        if (!graph.ContainsKey(methodEntry.Key))
                        {
                            graph[methodEntry.Key] = new List<string>();
                        }
                        graph[methodEntry.Key].Add(referenceEntry.Key);
                    }
                }
            }

            return graph;
        }

        private Dictionary<string, string> InitializeCommunities(Dictionary<string, List<string>> graph)
        {
            var communities = new Dictionary<string, string>();

            foreach (var vertex in graph.Keys)
            {
                communities[vertex] = vertex;
            }

            return communities;
        }

        private double CalculateModularityGain(string vertex, string community, Dictionary<string, string> communities, Dictionary<string, List<string>> graph)
        {
            // Calcula o grau do vértice
            int degree = graph[vertex].Count;

            // Calcula o grau total da comunidade
            int communityDegree = communities.Where(c => c.Value == community)
                                              .Sum(c => graph.ContainsKey(c.Key) ? graph[c.Key].Count : 0);

            // Calcula o número de arestas entre o vértice e a comunidade
            int edgesToCommunity = graph[vertex].Count(neighbor => communities.ContainsKey(neighbor) && communities[neighbor] == community);

            // Calcula o ganho de modularidade
            double m = graph.Sum(g => g.Value.Count) / 2.0; // Número total de arestas no grafo
            double deltaQ = (edgesToCommunity / m) - (degree * communityDegree / (2.0 * m * m));

            return deltaQ;
        }

        private Dictionary<string, List<string>> AggregateCommunities(Dictionary<string, List<string>> graph, Dictionary<string, string> communities)
        {
            var newGraph = new Dictionary<string, List<string>>();
            var communityMap = communities.GroupBy(c => c.Value).ToDictionary(g => g.Key, g => g.Select(c => c.Key).ToList());

            foreach (var community in communityMap)
            {
                var newVertex = community.Key;
                newGraph[newVertex] = new List<string>();

                foreach (var vertex in community.Value)
                {
                    if (graph.ContainsKey(vertex))
                    {
                        newGraph[newVertex].AddRange(graph[vertex].Where(v => communities.ContainsKey(v) && communities[v] != community.Key));
                    }
                }
            }

            return newGraph;
        }
    }
}
