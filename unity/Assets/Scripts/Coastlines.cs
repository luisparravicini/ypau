﻿using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Voronoi;
using Cell = Voronoi.Cell;

public class Coastlines : MonoBehaviour
{
    public int numSites = 36;
    public Bounds bounds;
    public int relaxationSteps;
    public GameObject chunkObj;
    public Gradient heightColors;
    public float heightDecay;
    public float sharpness;

    private List<Point> sites;
    private FortuneVoronoi voronoi;
    public VoronoiGraph graph;
    private List<FractureChunk> chunks;
    private Queue<FractureChunk> chunksPool;
    Heights heightMap;

    void Start()
    {
        sites = new List<Point>();
        voronoi = new FortuneVoronoi();
        chunks = new List<FractureChunk>();
        chunksPool = new Queue<FractureChunk>();

        CreateMap();
    }

    private void CreateMap()
    {
        var start = System.DateTime.Now;
        CreateSites(true, true, relaxationSteps);
        var finish = System.DateTime.Now;
        var elapsedSites = (finish - start).TotalSeconds;

        start = System.DateTime.Now;
        CreateHeights();
        finish = System.DateTime.Now;
        var elapsedHeights = (finish - start).TotalSeconds;

        start = System.DateTime.Now;
        CreateChunks();
        finish = System.DateTime.Now;
        var elapsedChunks = (finish - start).TotalSeconds;

        Debug.Log("sites:" + elapsedSites + "s"
        + ", heights:" + elapsedHeights + "s"
        + ", chunks:" + elapsedChunks + "s"
        + ", nSites:" + numSites);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            CreateMap();
        }
    }

    public void CreateMesh()
    {
        const int MaxMaterials = 10;
        var allVertices = new List<Vector3>();
        var allTriangs = new Dictionary<int, List<int>>();
        var meshFilter = GetComponent<MeshFilter>();
        var meshRenderer = GetComponent<MeshRenderer>();
        var mesh = meshFilter.mesh;

        var vertices = new List<Vector3>();
        var verticesHash = new Dictionary<Vector3, int>();
        var triangles = new List<int>();
        foreach (var cell in graph.cells)
        {
            if (cell.halfEdges.Count == 0) continue;

            var position = transform.position;

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
            vertices.Clear();
            triangles.Clear();
        }

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

    void CreateChunks()
    {
        CreateMesh();
        //foreach (var obj in chunks)
        //{
        //    obj.gameObject.SetActive(false);
        //    obj.transform.SetParent(poolContainer);
        //    chunksPool.Enqueue(obj);
        //}
        //chunks.Clear();

        //foreach (Cell cell in graph.cells)
        //{
        //    FractureChunk chunk;
        //    if (chunksPool.Count > 0)
        //    {
        //        chunk = chunksPool.Dequeue();
        //        chunk.gameObject.SetActive(true);
        //    }
        //    else
        //    {
        //        chunk = Instantiate(chunkObj, cell.site.ToVector3(), Quaternion.identity).GetComponent<FractureChunk>();
        //    }
        //    chunk.name = "Chunk " + cell.site.id;
        //    chunk.transform.SetParent(chunksContainer);
        //    chunks.Add(chunk);

        //    //fracChunk.CreateFanMesh(cell);
        //    chunk.CreateStipMesh(cell);
        //    chunk.SetColor(heightColors.Evaluate(heightMap.Height(cell.site)));
        //}
    }

    void CreateHeights()
    {
        heightMap = new Heights(graph, heightDecay, sharpness);
        heightMap.Create();
    }

    void Compute(List<Point> sites)
    {
        this.sites = sites;
        this.graph = this.voronoi.Compute(sites, this.bounds);
    }

    void CreateSites(bool clear = true, bool relax = false, int relaxCount = 2)
    {
        List<Point> sites = new List<Point>();
        if (!clear)
        {
            sites = this.sites.Take(this.sites.Count).ToList();
        }

        // create vertices
        for (int i = 0; i < numSites; i++)
        {
            Point site = new Point(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.z, bounds.max.z), 0);
            sites.Add(site);
        }

        Compute(sites);

        if (relax)
        {
            RelaxSites(relaxCount);
        }
    }

    void RelaxSites(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            if (!this.graph)
            {
                return;
            }

            Point site;
            List<Point> sites = new List<Point>();
            float dist = 0;

            float p = 1 / graph.cells.Count * 0.1f;

            for (int iCell = graph.cells.Count - 1; iCell >= 0; iCell--)
            {
                Voronoi.Cell cell = graph.cells[iCell];
                float rn = Random.value;

                // probability of apoptosis
                if (rn < p)
                {
                    continue;
                }

                site = CellCentroid(cell);
                dist = Distance(site, cell.site);

                // don't relax too fast
                if (dist > 2)
                {
                    site.x = (site.x + cell.site.x) / 2;
                    site.y = (site.y + cell.site.y) / 2;
                }
                // probability of mytosis
                if (rn > (1 - p))
                {
                    dist /= 2;
                    sites.Add(new Point(site.x + (site.x - cell.site.x) / dist, site.y + (site.y - cell.site.y) / dist));
                }
                sites.Add(site);
            }

            Compute(sites);
        }
    }

    float Distance(Point a, Point b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    Point CellCentroid(Voronoi.Cell cell)
    {
        float x = 0f;
        float y = 0f;
        Point p1, p2;
        float v;

        for (int iHalfEdge = cell.halfEdges.Count - 1; iHalfEdge >= 0; iHalfEdge--)
        {
            HalfEdge halfEdge = cell.halfEdges[iHalfEdge];
            p1 = halfEdge.GetStartPoint();
            p2 = halfEdge.GetEndPoint();
            v = p1.x * p2.y - p2.x * p1.y;
            x += (p1.x + p2.x) * v;
            y += (p1.y + p2.y) * v;
        }
        v = CellArea(cell) * 6;
        return new Point(x / v, y / v);
    }

    float CellArea(Voronoi.Cell cell)
    {
        float area = 0.0f;
        Point p1, p2;

        for (int iHalfEdge = cell.halfEdges.Count - 1; iHalfEdge >= 0; iHalfEdge--)
        {
            HalfEdge halfEdge = cell.halfEdges[iHalfEdge];
            p1 = halfEdge.GetStartPoint();
            p2 = halfEdge.GetEndPoint();
            area += p1.x * p2.y;
            area -= p1.y * p2.x;
        }
        area /= 2;
        return area;
    }

    void OnDrawGizmos()
    {
        if (graph)
        {
            foreach (Voronoi.Cell cell in graph.cells)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(new Vector3(cell.site.x, 0, cell.site.y), Vector3.one);

                if (cell.halfEdges.Count > 0)
                {
                    for (int i = 0; i < cell.halfEdges.Count; i++)
                    {
                        HalfEdge halfEdge = cell.halfEdges[i];

                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(halfEdge.GetStartPoint().ToVector3(),
                                        halfEdge.GetEndPoint().ToVector3());
                    }
                }
            }

            //foreach (var edge in graph.edges)
            //{
            //    if (edge.rSite != null)
            //    {
            //        Gizmos.color = Color.yellow;
            //        Gizmos.DrawLine(edge.lSite.ToVector3(), edge.rSite.ToVector3());
            //        Gizmos.DrawSphere(edge.lSite.ToVector3(), 0.2f);

            //        Gizmos.color = Color.green;
            //        Gizmos.DrawSphere(edge.rSite.ToVector3(), 0.2f);
            //    }
            //}
        }
    }
}
