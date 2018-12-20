using System;
using System.Collections.Generic;
using NivalTask.Pathfinding;
using UnityEngine;

namespace NivalTask
{
    public class BlockingCubeCreator : MonoBehaviour
    {
        public GameObject BlockingCubePrefab;

        public Field Field;

        private Dictionary<GameObject, GameObject> blockingCubeByCell 
            = new Dictionary<GameObject, GameObject>();

        private void Start()
        {
            Field.CellBlocked += OnCellBlocked;
            Field.CellUnblocked += OnCellUnblocked;
        }

        private void OnCellBlocked(Location loc, GameObject cell)
        {
            var cube = Instantiate(BlockingCubePrefab, cell.transform.position
                , cell.transform.rotation, cell.transform);
            blockingCubeByCell[cell] = cube;
        }

        private void OnCellUnblocked(Location loc, GameObject cell)
        {
            var cube = blockingCubeByCell[cell];
            Destroy(cube);
            blockingCubeByCell.Remove(cell);
        }
    }
}
