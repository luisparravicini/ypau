using System.Collections.Generic;
using Voronoi;
using UnityEngine;

public class Heights
{
    private VoronoiGraph graph;
    Dictionary<Point, float> heights;
    float decay;
    float sharpness;
    Dictionary<Point, HashSet<Point>> edges;

    public Heights(VoronoiGraph graph, float decay, float sharpness)
    {
        edges = new Dictionary<Point, HashSet<Point>>();
        this.graph = graph;
        this.decay = decay;
        this.sharpness = sharpness;
        heights = new Dictionary<Point, float>();
    }

    internal void Create()
    {
        SetupEdges();
        heights.Clear();

        var firstPoint = graph.edges[Random.Range(0, graph.edges.Count)].lSite;
        var height = Random.Range(0.75f, 1f);
        heights[firstPoint] = height;

        var queue = new Queue<Point>();
        EnqueueNeighbours(queue, firstPoint);

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            var modifier = Random.value * sharpness;
            if (Mathf.Approximately(modifier, 0))
                modifier = 1;
            var h = Mathf.Clamp(AverageNearHeights(p) * decay * modifier, 0, 1);
            heights[p] = h;

            EnqueueNeighbours(queue, p);
        }
    }

    private void SetupEdges()
    {
        foreach (var edge in graph.edges)
        {
            AddNeighbour(edge.lSite, edge.rSite);
            AddNeighbour(edge.rSite, edge.lSite);
        }
    }

    private void AddNeighbour(Point a, Point b)
    {
        if (a == null)
            return;
        if (!edges.ContainsKey(a))
            edges[a] = new HashSet<Point>();
        if (b != null)
            edges[a].Add(b);
    }

    private float AverageNearHeights(Point p)
    {
        float h = 0f;
        int n = 0;

        foreach (var neighbourPoint in edges[p])
        {
            if (heights.ContainsKey(neighbourPoint))
            {
                h += heights[neighbourPoint];
                n += 1;
            }
        }

        //if (n == 0)
        //throw new System.Exception("No height in neighbours");
        //if (n == 0)
        //n = 1;

        return h / n;
    }

    private void EnqueueNeighbours(Queue<Point> queue, Point point)
    {
        foreach (var edge in graph.edges)
        {
            if (edge.lSite == point)
            {
                var neighbour = edge.rSite;
                if (neighbour != null && !heights.ContainsKey(neighbour) && !queue.Contains(neighbour))
                    queue.Enqueue(neighbour);
            }
            if (edge.rSite == point)
            {
                var neighbour = edge.lSite;
                if (neighbour != null && !heights.ContainsKey(neighbour) && !queue.Contains(neighbour))
                    queue.Enqueue(neighbour);
            }
        }
    }

    internal float Height(Point site)
    {
        if (!heights.ContainsKey(site))
        {
            Debug.Log("no height!");
            return 1;
        }
        return heights[site];
        //return Random.Range(0f, 1f);
    }
}
