using System;
using System.Collections.Generic;
using System.IO;

// AVL Tree Node
public class AVLNode<T>
{
    public T Data { get; set; }
    public int Height { get; set; }
    public AVLNode<T> Left { get; set; }
    public AVLNode<T> Right { get; set; }

    public AVLNode(T data)
    {
        Data = data;
        Height = 1;
        Left = null;
        Right = null;
    }
}

// AVL Tree
public class AVLTree<T> where T : IComparable<T>
{
    private AVLNode<T> root;

    public AVLTree()
    {
        root = null;
    }

    private int GetHeight(AVLNode<T> node)
    {
        return node == null ? 0 : node.Height;
    }

    private int GetBalance(AVLNode<T> node)
    {
        return (node == null) ? 0 : GetHeight(node.Left) - GetHeight(node.Right);
    }

    private AVLNode<T> RotateRight(AVLNode<T> y)
    {
        AVLNode<T> x = y.Left;
        AVLNode<T> T2 = x.Right;

        x.Right = y;
        y.Left = T2;

        y.Height = Math.Max(GetHeight(y.Left), GetHeight(y.Right)) + 1;
        x.Height = Math.Max(GetHeight(x.Left), GetHeight(x.Right)) + 1;

        return x;
    }

    private AVLNode<T> RotateLeft(AVLNode<T> x)
    {
        AVLNode<T> y = x.Right;
        AVLNode<T> T2 = y.Left;

        y.Left = x;
        x.Right = T2;

        x.Height = Math.Max(GetHeight(x.Left), GetHeight(x.Right)) + 1;
        y.Height = Math.Max(GetHeight(y.Left), GetHeight(y.Right)) + 1;

        return y;
    }

    private AVLNode<T> Insert(AVLNode<T> node, T data)
    {
        if (node == null)
            return new AVLNode<T>(data);

        if (data.CompareTo(node.Data) < 0)
            node.Left = Insert(node.Left, data);
        else if (data.CompareTo(node.Data) > 0)
            node.Right = Insert(node.Right, data);
        else
            return node;

        node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

        int balance = GetBalance(node);

        if (balance > 1 && data.CompareTo(node.Left.Data) < 0)
            return RotateRight(node);

        if (balance < -1 && data.CompareTo(node.Right.Data) > 0)
            return RotateLeft(node);

        if (balance > 1 && data.CompareTo(node.Left.Data) > 0)
        {
            node.Left = RotateLeft(node.Left);
            return RotateRight(node);
        }

        if (balance < -1 && data.CompareTo(node.Right.Data) < 0)
        {
            node.Right = RotateRight(node.Right);
            return RotateLeft(node);
        }

        return node;
    }

    public void Insert(T data)
    {
        root = Insert(root, data);
    }
}

// Graph Edge
public class GraphEdge : IComparable<GraphEdge>
{
    public string Source { get; set; }
    public string Destination { get; set; }
    public int TravelTime { get; set; }

    public int CompareTo(GraphEdge other)
    {
        int sourceComparison = Source.CompareTo(other.Source);
        if (sourceComparison != 0)
            return sourceComparison;
        return Destination.CompareTo(other.Destination);
    }
}

// Graph
public class Graph
{
    private AVLTree<GraphEdge> edgeTree;
    private Dictionary<string, Dictionary<string, int>> edgeMap;

    public Graph()
    {
        edgeTree = new AVLTree<GraphEdge>();
        edgeMap = new Dictionary<string, Dictionary<string, int>>();
    }

    public void AddEdge(string source, string destination, int travelTime)
    {
        GraphEdge edge = new GraphEdge
        {
            Source = source,
            Destination = destination,
            TravelTime = travelTime
        };

        edgeTree.Insert(edge);

        if (!edgeMap.ContainsKey(source))
            edgeMap[source] = new Dictionary<string, int>();
        if (!edgeMap.ContainsKey(destination))
            edgeMap[destination] = new Dictionary<string, int>();

        edgeMap[source][destination] = travelTime;
        edgeMap[destination][source] = travelTime; // This makes the graph bidirectional
    }

    public (List<string>, int) FindShortestPath(string start, string end)
    {
        Dictionary<string, int> distances = new Dictionary<string, int>();
        Dictionary<string, string> previous = new Dictionary<string, string>();
        PriorityQueue<string, int> queue = new PriorityQueue<string, int>();

        foreach (var currentNode in edgeMap.Keys)
        {
            distances[currentNode] = int.MaxValue;
            previous[currentNode] = null;
        }
        distances[start] = 0;
        queue.Enqueue(start, 0);

        while (queue.Count > 0)
        {
            string currentNode = queue.Dequeue();
            if (currentNode == end)
                break;

            if (edgeMap.ContainsKey(currentNode))
            {
                foreach (var neighbor in edgeMap[currentNode])
                {
                    int distance = distances[currentNode] + neighbor.Value;
                    if (distance < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = distance;
                        previous[neighbor.Key] = currentNode;
                        queue.Enqueue(neighbor.Key, distance);
                    }
                }
            }
        }

        List<string> path = new List<string>();
        string node = end;
        int totalTravelTime = 0;
        while (node != null)
        {
            path.Insert(0, node);
            if (previous[node] != null)
            {
                totalTravelTime += edgeMap[previous[node]][node];
            }
            node = previous[node];
        }
        return (path, totalTravelTime);
    }

    public void PrintGraphInfo(string filePath)
{
    using (StreamWriter writer = new StreamWriter(filePath, false))
    {
        writer.WriteLine("Graph Information:");
        writer.WriteLine("==================");
        writer.WriteLine();

        foreach (var node in edgeMap.Keys)
        {
            writer.WriteLine($"Node: {node}");
            writer.WriteLine("Connected to:");

            foreach (var connection in edgeMap[node])
            {
                writer.WriteLine($"  - {connection.Key} (Travel Time: {connection.Value})");
            }

            writer.WriteLine();
        }
    }
}



    public void LoadDataFromFile(string filePath)
{
    string[] lines = File.ReadAllLines(filePath);
    bool isNewFormat = filePath.Contains("trip_results");

    foreach (string line in lines)
    {
        if (string.IsNullOrWhiteSpace(line))
            continue;

        string[] parts = line.Split(',');
        if (parts.Length < 3)
        {
            Console.WriteLine($"Invalid line format: {line}");
            continue;
        }

        try
        {
            string source, destination;
            int travelTime;

            if (isNewFormat)
            {
                // New format: UserID:101, From:Vanak, To:Tajrish, Path:Vanak->Tajrish, TravelTime:15
                source = parts[1].Trim().Split(':')[1];
                destination = parts[2].Trim().Split(':')[1];
                travelTime = int.Parse(parts[4].Trim().Split(':')[1]);
            }
            else
            {
                // Old format: RoadID:1, From:Vanak, To:Tajrish, Distance:5.0, Traffic:Moderate, TravelTime:15
                source = parts[1].Trim().Split(':')[1];
                destination = parts[2].Trim().Split(':')[1];
                travelTime = int.Parse(parts[5].Trim().Split(':')[1]);
            }

            AddEdge(source, destination, travelTime);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing line: {line}");
            Console.WriteLine($"Error message: {ex.Message}");
        }
    }
        
}




    public void ProcessRequests(string inputFilePath, string outputFilePath)
{
    string[] lines = File.ReadAllLines(inputFilePath);
    Queue<(string UserId, string From, string To)> requests = new Queue<(string, string, string)>();

    foreach (string line in lines)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            continue; // Skip empty lines
        }

        try
        {
            string[] parts = line.Split(',');
            if (parts.Length < 4)
            {
                Console.WriteLine($"Invalid line format: {line}");
                continue;
            }

            string userId = parts[1].Trim().Split(':')[1];
            string from = parts[2].Trim().Split(':')[1];
            string to = parts[3].Trim().Split(':')[1];
            requests.Enqueue((userId, from, to));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing line: {line}");
            Console.WriteLine($"Error message: {ex.Message}");
        }
    }

    using (StreamWriter writer = new StreamWriter(outputFilePath, true))
    {
        while (requests.Count > 0)
        {
            var (userId, from, to) = requests.Dequeue();
            try
            {
                var (shortestPath, totalTravelTime) = FindShortestPath(from, to);
                string result = $"UserID:{userId}, From:{from}, To:{to}, Path:{string.Join("->", shortestPath)}, TravelTime:{totalTravelTime}";
                
                Console.WriteLine(result);
                writer.WriteLine(result);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error finding path for UserID:{userId}, From:{from}, To:{to}. Error: {ex.Message}";
                Console.WriteLine(errorMessage);
                writer.WriteLine(errorMessage);
            }
        }
    }
}

}

class Program
{
    static void Main(string[] args)
    {
        Graph graph = new Graph();

        string inputFilePath = "C:\\programing\\projects\\graph-city-csharp\\avlgraph\\test_input.txt";
        string outputFilePath = "C:\\programing\\projects\\graph-city-csharp\\avlgraph\\trip_results.txt";
        string dataFilePath = "C:\\programing\\projects\\graph-city-csharp\\avlgraph\\road_data.txt";
        string graphFilePath = "C:\\programing\\projects\\graph-city-csharp\\avlgraph\\graph_data.txt";


       
        
        // Load old format data
        graph.LoadDataFromFile(dataFilePath);
        
        // Load new format data
        graph.LoadDataFromFile(outputFilePath);

        // Process requests
        graph.ProcessRequests(inputFilePath, outputFilePath);

        graph.PrintGraphInfo(graphFilePath);

    }
    
}

