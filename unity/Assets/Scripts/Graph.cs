using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CoastlinesGen
{

    public class Edge
    {
        public Vector3 startPoint;
        public Vector3 endPoint;

        public Edge(Vector3 startPoint, Vector3 endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }
    }

    public class Node
    {
        public Vector3 point;
        public HashSet<Node> neighbours;
        public List<Edge> edges;

        public Node(Vector3 pos)
        {
            neighbours = new HashSet<Node>();
            edges = new List<Edge>();
            point = pos;
        }
    }

    public class Graph
    {
        public List<Node> nodes;
        public Dictionary<Vector3, HashSet<Vector3>> edgeNeighbours;
        public List<Vector3> edgeNeighboursKeys;

        public Graph(Voronoi.VoronoiGraph graph)
        {
            nodes = new List<Node>();

            foreach (var cell in graph.cells)
            {
                var nodeSite = cell.site;
                var node = new Node(nodeSite.ToVector3());
                foreach (var edge in cell.halfEdges)
                {
                    var e = new Edge(edge.GetStartPoint().ToVector3(), edge.GetEndPoint().ToVector3());
                    node.edges.Add(e);
                }

                nodes.Add(node);
            }

            var edgeNeighboursAux = new Dictionary<Voronoi.Point, HashSet<Voronoi.Point>>();

            void AddNeighbour(Voronoi.Point a, Voronoi.Point b)
            {
                if (a == null || b == null)
                    return;

                if (!edgeNeighboursAux.ContainsKey(a))
                    edgeNeighboursAux[a] = new HashSet<Voronoi.Point>();
                edgeNeighboursAux[a].Add(b);
            };

            foreach (var node in graph.cells)
            {
                foreach (var edge in node.halfEdges)
                {
                    AddNeighbour(edge.GetStartPoint(), edge.GetEndPoint());
                    AddNeighbour(edge.GetEndPoint(), edge.GetStartPoint());
                }
            }
            edgeNeighbours = new Dictionary<Vector3, HashSet<Vector3>>();
            edgeNeighboursKeys = new List<Vector3>();
            foreach (Voronoi.Point k in edgeNeighboursAux.Keys)
            {
                var neighbours = new HashSet<Vector3>(edgeNeighboursAux[k].Select(x => x.ToVector3()));
                var p = k.ToVector3();
                edgeNeighbours[p] = neighbours;

                edgeNeighboursKeys.Add(p);
            }

            Debug.Log("graph: " + nodes.Count + " nodes, " + edgeNeighboursKeys.Count + " edgeNeighbours");
        }
    }


}
