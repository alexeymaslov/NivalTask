using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NivalTask.Pathfinding;

namespace NivalTask
{
    public class RushingToMarkedCellActivator : MonoBehaviour
    {
        public UnitCreator UnitCreator;
        public Field Field;
        public Navigator Navigator;

        private bool unitsAreRushing;
        private Dictionary<Unit, Location?> markedLocationByUnit = new Dictionary<Unit, Location?>();

        private void Awake()
        {
            UnitCreator.UnitCreated += OnUnitCreated;
            Field.CellUnblocked += OnCellUnblocked;
            Field.CellMarked += OnCellMarked;
            Field.CellUnmarked += OnCellUnmarked;
        }

        private void OnCellUnmarked(Location location, GameObject cell)
        {
            if (!unitsAreRushing) return;

            foreach (var unit in new HashSet<Unit>(markedLocationByUnit.Keys))
            {
                var maybeUnitLoc = markedLocationByUnit[unit];
                if (maybeUnitLoc.HasValue)
                {
                    var unitLoc = maybeUnitLoc.Value;
                    if (unitLoc.Equals(location))
                    {
                        Navigator.ClearPath(unit);
                        unit.EnterRushingState();
                        markedLocationByUnit[unit] = null;
                    }
                }
            }

            SetMarkedCellForUnitsWithoutMarkedCell();
        }

        private void OnCellMarked(Location location, GameObject cell)
        {
            if (!unitsAreRushing) return;
            
            SetMarkedCellForUnitsWithoutMarkedCell();
        }

        private void OnCellUnblocked(Location loc, GameObject cell)
        {
            if (!unitsAreRushing) return;
            
            SetMarkedCellForUnitsWithoutMarkedCell();
        }

        private void SetMarkedCellForUnitsWithoutMarkedCell()
        {
            List<Location> availableMarkedLocations = Field.GetUnblockedMarkedLocationsRandomlySorted();
            availableMarkedLocations.RemoveAll(l => markedLocationByUnit.Values.Contains(l));

            foreach (var unit in new HashSet<Unit>(markedLocationByUnit.Keys))
            {
                if (markedLocationByUnit[unit] == null)
                {
                    Navigator.ClearPath(unit);
                    unit.EnterRushingState();

                    foreach (var l in availableMarkedLocations)
                    {
                        if (Navigator.TryRequestPath(unit, l))
                        {
                            markedLocationByUnit[unit] = l;
                            availableMarkedLocations.Remove(l);
                            break;
                        }
                    }
                }
            }
        }

        private void OnUnitCreated(Unit unit)
        {
            markedLocationByUnit.Add(unit, null);
        }

        public void ToggleRushingToMarkedCell()
        {
            unitsAreRushing = !unitsAreRushing;

            if (unitsAreRushing)
            {
                StartRushing();
            }
            else
            {
                StopRushing();
            }
        }

        private void StartRushing()
        {
            SetMarkedCellForUnitsWithoutMarkedCell();
        }

        private void StopRushing()
        {
            foreach (var unit in new HashSet<Unit>(markedLocationByUnit.Keys))
            {
                markedLocationByUnit[unit] = null;
                unit.ExitRushingState();
                Navigator.ClearPath(unit);
            }
        }

        internal void HandleUnachievableMarkedLocation(Unit unit)
        {
            markedLocationByUnit[unit] = null;

            List<Location> availableMarkedLocations = Field.GetUnblockedMarkedLocationsRandomlySorted();
            availableMarkedLocations.RemoveAll(loc => markedLocationByUnit.Values.Contains(loc));

            Navigator.ClearPath(unit);
            unit.EnterRushingState();

            foreach (var loc in availableMarkedLocations)
            {
                if (Navigator.TryRequestPath(unit, loc))
                {
                    markedLocationByUnit[unit] = loc;
                    break;
                }
            }
        }
    }
}
