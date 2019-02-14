using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public Dictionary<Vector3, HashSet<Node>> positionsNeighbours;

        public Graph(Voronoi.VoronoiGraph graph)
        {
            //positionsNeighbours = new Dictionary<Vector3, HashSet<Node>>();
            nodes = new List<Node>();

            //var cellPositions = new Dictionary<Voronoi.Point, Node>();

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
                //cellPositions[nodeSite] = node;
            }


            //void AddNeighbour2(Voronoi.Point a, Node n)
            //{
            //    if (a == null)
            //        return;

            //    var aPos = a.ToVector3();
            //    if (!positionsNeighbours.ContainsKey(aPos))
            //        positionsNeighbours[aPos] = new HashSet<Node>();
            //    positionsNeighbours[aPos].Add(n);
            //};

            //foreach (var edge in graph.edges)
            //{
            //    AddNeighbour2(edge.lSite, node);
            //    AddNeighbour2(edge.rSite, node);
            //}

            //void AddNeighbour(Voronoi.Point a, Voronoi.Point b)
            //{
            //    if (a == null || b == null)
            //        return;

            //    cellPositions[a].neighbours.Add(cellPositions[b]);
            //};

            //foreach (var edge in graph.edges)
            //{
            //    AddNeighbour(edge.lSite, edge.rSite);
            //    AddNeighbour(edge.rSite, edge.lSite);
            //}

        }
    }


}
