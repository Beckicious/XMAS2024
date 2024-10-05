using System;
using System.Collections.Generic;

public record Point
{
    public int Y;
    public int X;

    public Point(int y, int x)
    {
        Y = y;
        X = x;
    }
}

public class AStar
{
    private PriorityQueue<Point, int> Queue;
    private XmasCell[,] Grid;

    private IEnumerable<Point> GetNeighbors(Point p)
    {
        // right
        if (p.X < Grid.GetLength(1) && Grid[p.Y, p.X + 1].Type == CellType.FREE)
        {
            yield return new Point(p.Y, p.X + 1);
        }
        // above
        if (p.Y > 0 && Grid[p.Y - 1, p.X].Type == CellType.FREE)
        {
            yield return new Point(p.Y - 1, p.X);
        }
        // below
        if (p.Y < Grid.GetLength(0) && Grid[p.Y + 1, p.X].Type == CellType.FREE)
        {
            yield return new Point(p.Y + 1, p.X);
        }
        // left
        if (p.X > 0 && Grid[p.Y, p.X - 1].Type == CellType.FREE)
        {
            yield return new Point(p.Y, p.X - 1);
        }
    }

    private int Heuristic(Point p, Point end)
    {
        return Math.Abs(p.Y - end.Y) + Math.Abs(p.X - end.X);
    }

    public AStar(XmasCell[,] grid)
    {
        Grid = grid;
    }

    public IList<Point> GetPath(Point start, Point end)
    {
        if (Grid[start.Y, start.X].Type != CellType.FREE)
        {
            return new List<Point>();
        }

        // for path reconstruction
        Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();

        // cheapest path length to node
        Dictionary<Point, int> gScore = new Dictionary<Point, int>();
        gScore.Add(start, 0);

        // current best guess
        Dictionary<Point, int> fScore = new Dictionary<Point, int>();
        fScore.Add(start, gScore[start] + Heuristic(start, end));

        Queue = new PriorityQueue<Point, int>();
        Queue.Enqueue(start, fScore[start]);

        while (Queue.Count > 0)
        {
            Point current = Queue.Dequeue();
            if (current == end)
            {
                IList<Point> path = new List<Point> { current };
                while (cameFrom.ContainsKey(current))
                {
                    current = cameFrom[current];
                    path.Add(current);
                }

                return path;
            }

            foreach (Point neighbor in GetNeighbors(current))
            {
                int neighborScore = gScore.GetValueOrDefault(current, int.MaxValue) + 1;
                if (neighborScore < gScore.GetValueOrDefault(neighbor, int.MaxValue))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = neighborScore;
                    int neighborFScore = neighborScore + Heuristic(neighbor, end);
                    fScore[neighbor] = neighborFScore;
                    if (!Queue.Contains(neighbor))
                    {
                        Queue.Enqueue(neighbor, neighborFScore);
                    }
                }
            }
        }

        return new List<Point>();
    }
}