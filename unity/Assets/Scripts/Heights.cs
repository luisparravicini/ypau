using System.Collections.Generic;
using UnityEngine;
using CoastlinesGen;
using System.Linq;

public class Heights
{
    private Graph graph;
    Dictionary<Vector3, float> heights;
    float decay;
    float sharpness;
    ISet<Vector3> visited;

    public Heights(Graph graph, float decay, float sharpness)
    {
        this.graph = graph;
        this.decay = decay;
        this.sharpness = sharpness;
        heights = new Dictionary<Vector3, float>();
        visited = new HashSet<Vector3>();
    }

    internal void Create()
    {
        heights.Clear();

        var firstPoint = graph.edgeNeighboursKeys[Random.Range(0, graph.edgeNeighboursKeys.Count)];
        UpdateHeights(firstPoint);
    }

    internal void AddTo(Vector3 p)
    {
        UpdateHeights(p);
    }

    void UpdateHeights(Vector3 firstPoint)
    {
        var height = Random.Range(0.75f, 1f);
        heights[firstPoint] = height;

        var queue = new Queue<Vector3>();
        queue.Enqueue(firstPoint);
        visited.Add(firstPoint);

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            visited.Add(p);

            EnqueueNeighbours(queue, p, (neighbour) =>
            {
                //var modifier = 1;
                var modifier = 1 - Random.value * sharpness;
                if (Mathf.Approximately(modifier, 0))
                    modifier = 1;
                var h = heights[p] * decay;
                if (heights.ContainsKey(neighbour))
                    h += heights[neighbour];
                h *= modifier;
                heights[neighbour] = Mathf.Clamp(h, 0, 1);
            });
        }
        visited.Clear();

        UpdateCenterHeights();
    }

    private void UpdateCenterHeights()
    {
        foreach (var node in graph.nodes)
        {
            var h = node.edges.Average(x => heights[x.startPoint]);
            heights[node.point] = h;
        }
    }

    private void EnqueueNeighbours(Queue<Vector3> queue, Vector3 point, System.Action<Vector3> action)
    {
        foreach (var neighbourPoint in graph.edgeNeighbours[point])
        {
            if (!visited.Contains(neighbourPoint) && !queue.Contains(neighbourPoint))
            {
                action(neighbourPoint);
                queue.Enqueue(neighbourPoint);
            }
        }
    }

    internal float Height(Vector3 site)
    {
        if (!heights.ContainsKey(site))
        {
            Debug.Log("no height!");
            return 0;
        }
        return heights[site];
    }
}
