using System;
using System.Collections.Generic;

namespace NivalTask.Pathfinding
{
    // Описывает квадратную сетку
    public class SquareGrid
    {
        // Размеры сетки в нодах
        public int Width { get; private set; }
        public int Height { get; private set; }

        // Множество непроходимых мест
        // Вроде как HashSet::Contains работает за О(1), так что имеет смысл 
        // использовать HashSet вместо, например, двумерного массива bool'ов
        public HashSet<Location> Walls { get; private set; }

        // Определяет направления к соседям
        private static readonly Location[] dirsToNeighbors = 
        {
            new Location(0, 1)
            , new Location(1, 1)
            , new Location(1, 0)
            , new Location(1, -1)
            , new Location(0, -1)
            , new Location(-1, -1)
            , new Location(-1, 0)
            , new Location(-1, 1)
        };

        public SquareGrid(int width, int height, HashSet<Location> walls = null)
        {
            if (width <= 0)
                throw new ArgumentException(
                    "width must be greater than 0", "width");

            if (height <= 0)
                throw new ArgumentException(
                    "height must be greater than 0", "height");

            Width = width;
            Height = height;

            if (walls != null)
            {
                Walls = walls;
                // Удалим стены, которые находятся вне границ сетки
                Walls.RemoveWhere(loc => !InBounds(loc));
            }
            else
                Walls = new HashSet<Location>();
        }

        public bool InBounds(Location id)
        {
            return 0 <= id.X && id.X < Width
                && 0 <= id.Y && id.Y < Height;
        }

        public bool Passable(Location id)
        {
            return !Walls.Contains(id);
        }

        // Чтобы работать в целых числах расстояния берутся умноженными на 10
        // Не проверяет, является ли b действительно соседом a
        public static int CostToNeighbor(Location a, Location b)
        {
            // true, когда b это сосед по диагонали к а
            if (a.X - b.X != 0
                && a.Y - b.Y != 0)
                return 14;
            
            return 10;
        }

        // Какие клетки считать соседними определяет dirsToNeighbors
        public IEnumerable<Location> Neighbors(Location id)
        {
            foreach (var dir in dirsToNeighbors)
            {
                var next = new Location(id.X + dir.X, id.Y + dir.Y);
                if (InBounds(next) && Passable(next))
                    yield return next;
            }
        }
    }
}
