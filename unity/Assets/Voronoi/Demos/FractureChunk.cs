using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Voronoi;
using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class FractureChunk : MonoBehaviour
{
    public Material material;

    private Mesh mesh;
    private Renderer _meshRenderer;

    private Renderer meshRenderer
    {
        get
        {
            if (_meshRenderer == null)
                _meshRenderer = GetComponent<Renderer>();
            return _meshRenderer;
        }
    }

    public void CreateFanMesh(Cell cell)
    {
        if (cell.halfEdges.Count > 0)
        {
            GetComponent<MeshFilter>().sharedMesh = mesh = new Mesh();
            mesh.name = "Chunk " + cell.site.id;

            Vector3[] vertices = new Vector3[cell.halfEdges.Count + 1];
            int[] triangles = new int[(cell.halfEdges.Count + 0) * 3];

            vertices[0] = cell.site.ToVector3() - transform.position;
            triangles[0] = 0;
            for (int v = 1, t = 1; v < vertices.Length; v++, t += 3)
            {
                vertices[v] = cell.halfEdges[v - 1].GetStartPoint().ToVector3() - transform.position;
                triangles[t] = v;
                triangles[t + 1] = v + 1;
            }
            triangles[triangles.Length - 1] = 1;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            GetComponent<MeshCollider>().sharedMesh = mesh;
            meshRenderer.sharedMaterial = material;
        }
    }

    public void CreateStipMesh(Cell cell)
    {
        if (cell.halfEdges.Count > 0)
        {
            GetComponent<MeshFilter>().sharedMesh = mesh = new Mesh();
            mesh.name = "Chunk " + cell.site.id;

            Vector3[] vertices = new Vector3[cell.halfEdges.Count + 1];
            int[] triangles = new int[(cell.halfEdges.Count + 0) * 3];

            vertices[0] = cell.site.ToVector3() - transform.position;
            triangles[0] = 0;
            for (int v = 1, t = 1; v < vertices.Length; v++, t += 3)
            {
                vertices[v] = cell.halfEdges[v - 1].GetStartPoint().ToVector3() - transform.position;
                triangles[t] = v;
                triangles[t + 1] = v + 1;
            }
            triangles[triangles.Length - 1] = 1;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            meshRenderer.sharedMaterial = material;
        }
    }

    internal void SetColor(Color c)
    {
        meshRenderer.material.color = c;
    }
}