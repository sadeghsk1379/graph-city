using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using GraphX.Common.Models;
using QuickGraph;
using ZedGraph;

namespace GraphProject
{
    public class Node : VertexBase
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Edge : EdgeBase<Node>
    {
        public int Weight { get; set; }

        public Edge(Node source, Node target, int weight)
            : base(source, target)
        {
            Weight = weight;
        }
    }

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static BidirectionalGraph<Node, Edge> ReadGraphFromFile(string filePath)
        {
            var graph = new BidirectionalGraph<Node, Edge>();
            Node currentNode = null;
            var nodeDictionary = new Dictionary<string, Node>();

            using (var file = new StreamReader(filePath))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("Node:"))
                    {
                        var nodeName = line.Split(':')[1].Trim();
                        if (!nodeDictionary.ContainsKey(nodeName))
                        {
                            currentNode = new Node { Name = nodeName };
                            nodeDictionary[nodeName] = currentNode;
                            graph.AddVertex(currentNode);
                        }
                        currentNode = nodeDictionary[nodeName];
                    }
                    else if (line.StartsWith("-"))
                    {
                        var parts = line.Split('(');
                        var connectedNodeName = parts[0].Trim('-').Trim();
                        int travelTime = int.Parse(parts[1].Split(':')[1].Trim(')'));

                        if (!nodeDictionary.ContainsKey(connectedNodeName))
                        {
                            nodeDictionary[connectedNodeName] = new Node { Name = connectedNodeName };
                            graph.AddVertex(nodeDictionary[connectedNodeName]);
                        }

                        var connectedNode = nodeDictionary[connectedNodeName];
                        var edge = new Edge(currentNode, connectedNode, travelTime);
                        graph.AddEdge(edge);
                    }
                }
            }
            return graph;
        }

        public static void DrawGraph(BidirectionalGraph<Node, Edge> graph, ZedGraphControl zgc)
        {
            var pane = zgc.GraphPane;
            pane.Title.Text = "Graph of Tehran's Districts";
            pane.XAxis.Title.Text = "X";
            pane.YAxis.Title.Text = "Y";

            var nodePositions = new Dictionary<Node, PointPair>();
            var rnd = new Random();

            foreach (var node in graph.Vertices)
            {
                var point = new PointPair(rnd.NextDouble(), rnd.NextDouble());
                nodePositions[node] = point;
                var text = new TextObj(node.Name, point.X, point.Y)
                {
                    FontSpec = { Border = { IsVisible = false } }
                };
                pane.GraphObjList.Add(text);
            }

            foreach (var edge in graph.Edges)
            {
                var sourcePos = nodePositions[edge.Source];
                var targetPos = nodePositions[edge.Target];
                var line = new LineObj(sourcePos.X, sourcePos.Y, targetPos.X, targetPos.Y)
                {
                    Line = { Color = System.Drawing.Color.Blue }
                };
                pane.GraphObjList.Add(line);
                var midPoint = new PointPair((sourcePos.X + targetPos.X) / 2, (sourcePos.Y + targetPos.Y) / 2);
                var text = new TextObj(edge.Weight.ToString(), midPoint.X, midPoint.Y)
                {
                    FontSpec = { Border = { IsVisible = false } }
                };
                pane.GraphObjList.Add(text);
            }

            zgc.AxisChange();
            zgc.Invalidate();
        }
    }
}
