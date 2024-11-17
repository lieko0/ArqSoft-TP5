using System;
using System.Collections.Generic;
using System.Linq;

namespace StructuralAnalyzer
{
    public class LouvainAlgorithm
    {
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, int>>> _callMatrix;

        public LouvainAlgorithm(Dictionary<string, Dictionary<string, Dictionary<string, int>>> callMatrix)
        {
            _callMatrix = callMatrix;
        }

        public LouvainAlgorithm(Dictionary<string, Dictionary<string, Dictionary<string, int>>> callMatrix, Dictionary<string, string> communities)
        {
            // Correct the callMatrix/graph so that represents the graph of the communities agregation
            _callMatrix = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
            foreach (var classEntry in callMatrix)
            {
                foreach (var methodEntry in classEntry.Value)
                {
                    foreach (var referenceEntry in methodEntry.Value)
                    {
                        if (communities.ContainsKey(methodEntry.Key) && communities.ContainsKey(referenceEntry.Key))
                        {
                            if (!_callMatrix.ContainsKey(communities[methodEntry.Key]))
                            {
                                _callMatrix[communities[methodEntry.Key]] = new Dictionary<string, Dictionary<string, int>>();
                            }
                            if (!_callMatrix[communities[methodEntry.Key]].ContainsKey(communities[referenceEntry.Key]))
                            {
                                _callMatrix[communities[methodEntry.Key]][communities[referenceEntry.Key]] = new Dictionary<string, int>();
                            }
                            if (!_callMatrix[communities[methodEntry.Key]][communities[referenceEntry.Key]].ContainsKey(methodEntry.Key))
                            {
                                _callMatrix[communities[methodEntry.Key]][communities[referenceEntry.Key]][methodEntry.Key] = 0;
                            }
                            _callMatrix[communities[methodEntry.Key]][communities[referenceEntry.Key]][methodEntry.Key] += referenceEntry.Value;
                        }
                    }
                }
            }

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
            int degree = graph[vertex].Count;
            int communityDegree = communities.Where(c => c.Value == community)
                                              .Sum(c => graph.ContainsKey(c.Key) ? graph[c.Key].Count : 0);
            int edgesToCommunity = graph[vertex].Count(neighbor => communities.ContainsKey(neighbor) && communities[neighbor] == community);

            double m = graph.Sum(g => g.Value.Count) / 2.0;
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
                        foreach (var neighbor in graph[vertex])
                        {
                            if (communities.ContainsKey(neighbor) && communities[neighbor] != community.Key)
                            {
                                newGraph[newVertex].Add(communities[neighbor]);
                            }
                        }
                    }
                }
            }

            return newGraph;
        }
    }
}
