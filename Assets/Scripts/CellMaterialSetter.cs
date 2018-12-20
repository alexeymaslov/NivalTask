using UnityEngine;
using NivalTask.Pathfinding;
using System;

namespace NivalTask
{
    public class CellMaterialSetter : MonoBehaviour
    {
        public Field Field;

        public Material DefaultCellMaterial;
        public Material MarkedCellMaterial;

        private void Start()
        {
            Field.CellMarked += OnCellMarked;
            Field.CellUnmarked += OnCellUnmarked;
        }

        private void OnCellMarked(Location location, GameObject cell)
        {
            cell.GetComponentInChildren<Renderer>().material = MarkedCellMaterial;
        }

        private void OnCellUnmarked(Location location, GameObject cell)
        {
            cell.GetComponentInChildren<Renderer>().material = DefaultCellMaterial;
        }
    }
}
