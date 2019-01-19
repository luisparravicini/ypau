using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Voronoi;
using System;

public class MeshGenerator
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    VoronoiGraph graph;
    private Heights heightMap;
    private Gradient heightColors;
    Vector3 position;
    private List<Vector3> vertices;
    private Dictionary<Vector3, int> verticesHash;
    private List<int> triangles;
    private List<Vector3> allVertices;
    private Dictionary<int, List<int>> allTriangs;
    const int MaxMaterials = 10;

    public MeshGenerator(Heights heightMap, Gradient colors, Vector3 position, VoronoiGraph graph, MeshFilter meshFilter, MeshRenderer meshRenderer)
    {
        this.heightMap = heightMap;
        this.heightColors = colors;
        this.position = position;
        this.graph = graph;
        this.meshFilter = meshFilter;
        this.meshRenderer = meshRenderer;
        mesh = meshFilter.mesh;

    }

    internal void Create()
    {
        allVertices = new List<Vector3>();
        allTriangs = new Dictionary<int, List<int>>();
        verticesHash = new Dictionary<Vector3, int>();

        vertices = new List<Vector3>();
        triangles = new List<int>();

        foreach (var cell in graph.cells)
        {
            if (CreateCellMesh(cell))
                CopySubmesh(cell);

            vertices.Clear();
            triangles.Clear();
        }

        SendMesh(allVertices, allTriangs);
        CreateMaterials();
    }

    private void CopySubmesh(Cell cell)
    {

        foreach (var vertex in vertices)
        {
            if (!verticesHash.ContainsKey(vertex))
            {
                verticesHash[vertex] = allVertices.Count;
                allVertices.Add(vertex);
            }
        }
        var submesh = Mathf.RoundToInt(heightMap.Height(cell.site) * MaxMaterials);
        if (!allTriangs.ContainsKey(submesh))
            allTriangs[submesh] = new List<int>();
        foreach (var index in triangles)
        {
            var i = verticesHash[vertices[index]];
            allTriangs[submesh].Add(i);
        }
    }

    private bool CreateCellMesh(Cell cell)
    {
        if (cell.halfEdges.Count == 0) return false;

        vertices.Add(cell.site.ToVector3() - position);
        triangles.Add(0);
        var lastV = cell.halfEdges.Count;
        for (int v = 1; v <= lastV; v++)
        {
            vertices.Add(cell.halfEdges[v - 1].GetStartPoint().ToVector3() - position);

            triangles.Add(v);
            if (v != lastV)
            {
                triangles.Add(v + 1);
                triangles.Add(0);
            }
        }
        triangles.Add(1);

        return true;
    }

    private void SendMesh(List<Vector3> allVertices, Dictionary<int, List<int>> allTriangs)
    {
        var triangCount = allTriangs.Values.Sum(values => values.Count);
        Debug.Log("mesh vertices: " + allVertices.Count + ", triangles:" + triangCount);

        mesh.Clear();
        mesh.vertices = allVertices.ToArray();

        mesh.subMeshCount = MaxMaterials;
        var keys = allTriangs.Keys.ToArray<int>();
        System.Array.Sort<int>(keys);
        foreach (var index in keys)
        {
            mesh.SetIndices(allTriangs[index].ToArray(), MeshTopology.Triangles, index);
        }

        mesh.RecalculateBounds();
    }

    private void CreateMaterials()
    {
        var materials = new List<Material>();
        var heightMaterial = Resources.Load<Material>("Height");
        while (materials.Count < MaxMaterials)
            materials.Add(heightMaterial);
        meshRenderer.materials = materials.ToArray();

        var colorIndex = 0f;
        foreach (var m in meshRenderer.materials)
        {
            m.color = heightColors.Evaluate(colorIndex);
            colorIndex += 1f / MaxMaterials;
        }
    }
}
