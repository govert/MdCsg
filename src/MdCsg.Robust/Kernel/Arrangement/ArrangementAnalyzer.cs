namespace MdCsg.Robust.Kernel.Arrangement;

public static class ArrangementAnalyzer
{
    public static ArrangementAnalysis Analyze(ArrangementGraph graph)
    {
        int endpointCount = CountEndpointVertices(graph);
        int componentCount = CountConnectedComponents(graph);

        return new ArrangementAnalysis(endpointCount, componentCount);
    }

    private static int CountEndpointVertices(ArrangementGraph graph)
    {
        int count = 0;
        foreach (var kvp in graph.IncidentEdgesByVertex)
        {
            if (kvp.Value.Count == 1)
                count++;
        }
        return count;
    }

    private static int CountConnectedComponents(ArrangementGraph graph)
    {
        if (graph.Vertices.Count == 0 || graph.Edges.Count == 0)
            return 0;

        var adjacency = new Dictionary<int, List<int>>(graph.Vertices.Count);
        for (int i = 0; i < graph.Vertices.Count; i++)
            adjacency[i] = [];

        foreach (var edge in graph.Edges)
        {
            adjacency[edge.StartVertexId].Add(edge.EndVertexId);
            if (edge.EndVertexId != edge.StartVertexId)
                adjacency[edge.EndVertexId].Add(edge.StartVertexId);
        }

        var visited = new HashSet<int>();
        int components = 0;
        foreach (var vertex in graph.Vertices)
        {
            if (visited.Contains(vertex.Id))
                continue;

            if (adjacency[vertex.Id].Count == 0)
                continue;

            components++;
            var queue = new Queue<int>();
            queue.Enqueue(vertex.Id);
            visited.Add(vertex.Id);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                foreach (int next in adjacency[current])
                {
                    if (visited.Add(next))
                        queue.Enqueue(next);
                }
            }
        }

        return components;
    }
}
