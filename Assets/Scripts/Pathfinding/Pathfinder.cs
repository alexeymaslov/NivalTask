using System;
using System.Collections.Generic;

namespace NivalTask.Pathfinding
{
    public class Pathfinder 
    {
        private Dictionary<Location, Location> cameFrom 
            = new Dictionary<Location, Location>();

        private Dictionary<Location, int> costSoFar 
            = new Dictionary<Location, int>();

        private PriorityQueue<Location> frontier 
            = new PriorityQueue<Location>();

        private SquareGrid grid;

        private Location start;

        private Location goal;

        public Pathfinder(SquareGrid grid)
        {
            if (grid == null)
                throw new ArgumentNullException("grid");

            this.grid = grid;
        }

        public bool FindPath(Location start, Location goal)
        {
            if (!grid.InBounds(start))
                throw new ArgumentException("grid does not contain start node"
                    , "start");

            if (!grid.InBounds(goal))
                throw new ArgumentException("grid does not contain goal node"
                    , "goal");

            if (!grid.Passable(start))
                throw new ArgumentException("start is impassable", "start");

            this.start = start;
            this.goal = goal;

            Init();
            return TryToFindPath();
        }

        private void Init()
        {
            frontier.Clear();
            costSoFar.Clear();

            frontier.Enqueue(start, 0);

            cameFrom[start] = start;
            costSoFar[start] = 0;
        }

        private bool TryToFindPath()
        {
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goal))
                {
                    return true;
                }

                foreach (var next in grid.Neighbors(current))
                {
                    int newCost = costSoFar[current] 
                        + SquareGrid.CostToNeighbor(current, next);
                    if (!costSoFar.ContainsKey(next) 
                        || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        int priority = newCost + Heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }

            return false;
        }

        // Возвращает true, если удалось найти путь до цели
        // Возвращает false, если удалось построить только частичный путь
        public bool FindPath(Location start, Location goal, List<Location> path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            bool pathFound = FindPath(start, goal);

            if (pathFound)
                TracePath(goal, path);
            else
            {
                Location closest = FindClosestToGoal();
                TracePath(closest, path);
            }

            path.Reverse();

            return pathFound;
        }

        // Захардкожено под прямоугольную сетку
        private int Heuristic(Location a, Location b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            dx = dx < 0 ? -dx : dx;
            dy = dy < 0 ? -dy : dy;

            return dx < dy ? 14 * dx + 10 * (dy - dx)
                : 14 * dy + 10 * (dx - dy);
        }

        private void TracePath(Location from, List<Location> path)
        {
            path.Add(from);
            var node = from;
            while (!node.Equals(start))
            {
                node = cameFrom[node];
                path.Add(node);
            }
        }

        private Location FindClosestToGoal()
        {
            var bestLoc = new Location(0, 0);
            int bestCost = int.MaxValue;
            foreach (var entry in costSoFar)
            {
                if (entry.Value < bestCost)
                {
                    bestLoc = entry.Key;
                    bestCost = entry.Value;
                }
            }
            return bestLoc;
        }
    }
}
