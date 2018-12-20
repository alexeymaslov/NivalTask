using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NivalTask.Pathfinding;
using Random = UnityEngine.Random;

namespace NivalTask
{
    [RequireComponent(typeof(BoxCollider))]
    public class Field : MonoBehaviour
    {
        public event Action<Location, GameObject> CellMarked;
        public event Action<Location, GameObject> CellUnmarked;
        public event Action<Location, GameObject> CellBlocked;
        public event Action<Location, GameObject> CellUnblocked;

        public int MinCellCount = 5;
        public int MaxCellCount = 10;

        // Длина стороны клетки
        public float CellSize = 1f;
        public GameObject CellPrefab;

        // Размер поля в единицах измерения юнити
        private Vector3 size;
        private Vector3 bottomLeft;

        private SquareGrid grid;

        private int[,] unitCountOnCell;

        private Dictionary<Location, GameObject> cellByLocation = new Dictionary<Location, GameObject>();

        // Длина стороны поля в клетках
        public int CellCount { get; private set; }
        public Pathfinder Pathfinder { get; private set; }

        public List<Location> UnblockedLocations { get; private set; } = new List<Location>();
        public List<Location> MarkedLocations { get; private set; } = new List<Location>();

        private void Awake()
        {
            CellCount = Random.Range(MinCellCount, MaxCellCount + 1);
            unitCountOnCell = new int[CellCount, CellCount];
            size = new Vector3(CellCount, 0f, CellCount) * CellSize;
            bottomLeft = transform.position - size / 2f;
            grid = new SquareGrid(CellCount, CellCount);
            Pathfinder = new Pathfinder(grid);

            AdjustColliderSize();
            InstantiateCells();
        }

        private void AdjustColliderSize()
        {
            var coll = GetComponent<BoxCollider>();
            var newCollSize = size;
            newCollSize.y = coll.size.y;
            coll.size = newCollSize;
        }

        private void InstantiateCells()
        {
            Vector3 cellCenterOffset = new Vector3(CellSize / 2f, 0f, CellSize / 2f);

            for (int i = 0; i < CellCount; i++)
            {
                for (int j = 0; j < CellCount; j++)
                {
                    Vector3 pos = bottomLeft 
                        + new Vector3(i, 0f, j) * CellSize + cellCenterOffset;
                    var cell = Instantiate(
                        CellPrefab, pos, Quaternion.identity, transform);
                    cell.name = $"Cell {i}, {j}";

                    var loc = new Location(i, j);
                    UnblockedLocations.Add(loc);
                    cellByLocation[loc] = cell;
                }
            }
        }

        public void Occupy(Vector3 point)
        {
            var maybeLoc = LocationByPoint(point);
            if (maybeLoc.HasValue)
            {
                Occupy(maybeLoc.Value);
            }
        }

        public void Occupy(Location loc)
        {
            unitCountOnCell[loc.X, loc.Y]++;
        }

        public void Unoccupy(Vector3 point)
        {
            var maybeLoc = LocationByPoint(point);
            if (maybeLoc.HasValue)
            {
                Unoccupy(maybeLoc.Value);
            }
        }

        public void Unoccupy(Location loc)
        {
            unitCountOnCell[loc.X, loc.Y]--;
        }

        private bool IsOccupied(Location loc)
        {
            return unitCountOnCell[loc.X, loc.Y] > 0;
        }

        public Location? LocationByPoint(Vector3 point)
        {
            Vector3 ij = (point - bottomLeft) / CellSize;
            int i = Mathf.FloorToInt(ij.x);
            int j = Mathf.FloorToInt(ij.z);

            var loc = new Location(i, j); 

            if (!grid.InBounds(loc))
                return null;
                
            return loc;
        }

        public Vector3? PointByLocation(Location loc)
        {
            if (!grid.InBounds(loc))
                return null;

            Vector3 offset = new Vector3(loc.X, 0f, loc.Y) * CellSize;
            Vector3 cellCenterOffset = new Vector3(CellSize, 0f, CellSize) / 2f;

            return bottomLeft + offset + cellCenterOffset;
        }

        public void Mark(Vector3 point)
        {
            var maybeLoc = LocationByPoint(point);
            if (maybeLoc.HasValue)
            {
                var loc = maybeLoc.Value;

                if (IsOccupied(loc)) return;

                if (MarkedLocations.Contains(loc))
                {
                    MarkedLocations.Remove(loc);
                    CellUnmarked?.Invoke(loc, cellByLocation[loc]);
                }
                else
                {
                    MarkedLocations.Add(loc);
                    CellMarked?.Invoke(loc, cellByLocation[loc]);
                }
            }
            else
            {
                Debug.LogWarning($"Trying to mark point: {point}, " 
                    + "but there is no such location on a grid.");
            }
        }

        public void Block(Vector3 point)
        {
            var maybeLoc = LocationByPoint(point);
            if (maybeLoc.HasValue)
            {
                var loc = maybeLoc.Value;

                if (IsOccupied(loc)) return;

                if (UnblockedLocations.Contains(loc))
                {
                    UnblockedLocations.Remove(loc);
                    grid.Walls.Add(loc);
                    CellBlocked?.Invoke(loc, cellByLocation[loc]);
                }
            }
            else
            {
                Debug.LogWarning($"Trying to block point: {point}, " 
                    + "but there is no such location on a grid.");
            }
        }

        public void Unblock(Vector3 point)
        {
            var maybeLoc = LocationByPoint(point);
            if (maybeLoc.HasValue)
            {
                var loc = maybeLoc.Value;

                if (IsOccupied(loc)) return;

                if (grid.Walls.Contains(loc))
                {
                    grid.Walls.Remove(loc);
                    UnblockedLocations.Add(loc);
                    CellUnblocked?.Invoke(loc, cellByLocation[loc]);
                }
            }
            else
            {
                Debug.LogWarning($"Trying to unblock point: {point}, " 
                    + "but there is no such location on a grid.");
            }
        }

        public Vector3? PickRandomUnblockedPoint()
        {
            var loc = PickRandomUnblockedLocation();
            if (loc.HasValue)
            {
                var maybePoint = PointByLocation(loc.Value);
                if (maybePoint.HasValue)
                {
                    return maybePoint.Value;
                }
            }

            return null;
        }

        public Location? PickRandomUnblockedLocation()
        {
            int c = UnblockedLocations.Count;
            if (c > 0)
            {
                return UnblockedLocations[Random.Range(0, c)];
            }

            return null;
        }

        public List<Location> GetUnblockedMarkedLocationsRandomlySorted()
        {
            return new List<Location>(MarkedLocations).Intersect(UnblockedLocations)
                .OrderBy(loc => Random.value).ToList();
        }
    }
}
