using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NivalTask
{
    public class UnitCreator : MonoBehaviour
    {
        public event Action<Unit> UnitCreated;

        public int MinUnitCount = 1;
        public int MaxUnitCount = 5;

        public Unit UnitPrefab;

        public Field Field;
        public Navigator Navigator;

        void Start()
        {
            // Spawn units
            int unitCount = Random.Range(MinUnitCount, MaxUnitCount + 1);

            for (int i = 0; i < unitCount; i++)
            {
                var maybePoint = Field.PickRandomUnblockedPoint();
                if (maybePoint.HasValue)
                {
                    var point = maybePoint.Value;
                    var unit = Instantiate<Unit>(UnitPrefab, point, Quaternion.identity);
                    unit.name = $"Unit {i}";
                    unit.Navigator = Navigator;
                    unit.Field = Field;
                    Field.Occupy(point);
                    UnitCreated?.Invoke(unit);
                }
            }
        }
    }
}
