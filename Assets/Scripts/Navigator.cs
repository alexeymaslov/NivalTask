using System;
using System.Collections.Generic;
using UnityEngine;
using NivalTask.Pathfinding;

namespace NivalTask
{
    public class Navigator : MonoBehaviour
    {
        public Field Field;
        public RushingToMarkedCellActivator rushingActivator;

        private Pathfinder pathfinder;

        private Dictionary<Unit, List<Location>> pathByUnit = new Dictionary<Unit, List<Location>>();

        private void Start()
        {
            pathfinder = Field.Pathfinder;
            Field.CellBlocked += OnCellBlocked;
        }

        private void OnCellBlocked(Location loc, GameObject cell)
        {
            foreach (var unit in new HashSet<Unit>(pathByUnit.Keys))
            {
                var path = pathByUnit[unit];
                if (path != null && path.Contains(loc)) // Путь возможно неверен
                {
                    // Заблокированная клетка оказалась на предстоящей части пути
                    if (unit.CurrentPathPointIndex <= path.IndexOf(loc))
                    {
                        if (unit.State == Unit.UnitState.Wandering)
                        {
                            unit.EnterIdleState();
                            pathByUnit[unit] = null;
                        }
                        else if (unit.State == Unit.UnitState.RushingToMarkedCell)
                        {
                            var maybeStart = Field.LocationByPoint(unit.transform.position);
                            if (maybeStart.HasValue)
                            {
                                var start = maybeStart.Value;
                                if (Field.UnblockedLocations.Contains(start))
                                {
                                    List<Location> locs = new List<Location>();
                                    if (pathfinder.FindPath(start, path[path.Count - 1], locs))
                                    {
                                        pathByUnit[unit] = locs;

                                        unit.Path = TransformLocationsIntoPoints(locs);
                                        unit.CurrentPathPointIndex = 0;
                                    }
                                    else
                                    {
                                        rushingActivator.HandleUnachievableMarkedLocation(unit);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Возвращает true, если удалось найти хоть какой-то путь 
        // (т.е. всегда когда юнит не стоит на заблокированной клетке)
        public bool TryRequestPathToRandomPoint(Unit unit)
        {
            var maybeLoc = Field.LocationByPoint(unit.transform.position);
            if (maybeLoc.HasValue)
            {
                var start = maybeLoc.Value;
                if (!Field.UnblockedLocations.Contains(start)) 
                    return false;

                var maybeGoalLoc = Field.PickRandomUnblockedLocation();
                if (maybeGoalLoc.HasValue)
                {
                    List<Location> locs = new List<Location>();
                    pathfinder.FindPath(start, maybeGoalLoc.Value, locs);
                    pathByUnit[unit] = locs;

                    unit.Path = TransformLocationsIntoPoints(locs);

                    return true;
                }
            }
            else
            {
                Debug.LogWarning("Path requested by unit which is not on the field.", unit);
            }

            return false;
        }

        // Возвращает true только если удалось найти полный путь до цели,
        // Возвращает false если не удалось найти путь или найден частичный путь
        public bool TryRequestPath(Unit unit, Location goal)
        {
            var maybeStart = Field.LocationByPoint(unit.transform.position);
            if (maybeStart.HasValue)
            {
                var start = maybeStart.Value;
                if (!Field.UnblockedLocations.Contains(start)) 
                    return false;

                List<Location> locs = new List<Location>();
                if (pathfinder.FindPath(start, goal, locs))
                {
                    pathByUnit[unit] = locs;

                    unit.Path = TransformLocationsIntoPoints(locs);

                    return true;
                }
            }
            else
            {
                Debug.LogWarning("Path requested by unit which is not on the field.", unit);
            }

            return false;
        }

        private List<Vector3> TransformLocationsIntoPoints(List<Location> locs)
        {
            List<Vector3> points = new List<Vector3>();
            foreach (var loc in locs)
            {
                var maybePoint = Field.PointByLocation(loc);
                if (maybePoint.HasValue)
                {
                    points.Add(maybePoint.Value);
                }
            }

            return points;
        }

        public void ClearPath(Unit unit)
        {
            pathByUnit[unit] = null;
        }
    }
}
