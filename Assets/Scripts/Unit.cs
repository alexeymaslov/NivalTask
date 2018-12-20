using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NivalTask
{
    public class Unit : MonoBehaviour
    {
        public enum UnitState { Idle, Wandering, RushingToMarkedCell }

        public float minTimeForTurn = 0.1f;
        public float maxTimeForTurn = 0.75f;


        public UnitState State { get; private set; }

        public Navigator Navigator { get; set; }
        public Field Field { get; set; }

        public List<Vector3> Path { get; set; }
        public int CurrentPathPointIndex { get; set; }

        public float TimeForTurn { get; private set; }
        public float TurnTimer { get; private set; }

        private void Awake()
        {
            TimeForTurn = Random.Range(minTimeForTurn, maxTimeForTurn);
        }

        private void Update()
        {
            TurnTimer += Time.deltaTime;
            if (TurnTimer > TimeForTurn)
            {
                TurnTimer -= TimeForTurn;
                OnUpdate();
            }
        }

        #region Debug
            
        Color[] debugGizmosColor = new Color[] { Color.black, Color.cyan, Color.gray, Color.green, Color.white };
        private void OnDrawGizmos()
        {
            if (name.Length >= 5)
            {
                Gizmos.color = debugGizmosColor[Int32.Parse(name.Substring(5))];
                if (Path == null) return;
                foreach (var p in Path)
                {
                    Gizmos.DrawSphere(p, .05f);
                }
            }
        }
        
        #endregion

        private void OnUpdate()
        {
            switch (State)
            {
                case UnitState.Idle:
                    HandleIdleState();
                break;
                case UnitState.Wandering:
                    HandleWanderingState();
                break;
                case UnitState.RushingToMarkedCell:
                    HandleRushingToMarkedCellState();
                break;
            }
        }

        private void HandleRushingToMarkedCellState()
        {
            if (Path == null)
            {
                CurrentPathPointIndex = 0;
            }
            else
            {
                if (CurrentPathPointIndex < Path.Count)
                {
                    Field.Unoccupy(transform.position);
                    transform.position = Path[CurrentPathPointIndex];
                    Field.Occupy(transform.position);
                    CurrentPathPointIndex++;
                }
                else // Уже находимся в конце пути
                {

                }
            }
        }

        private void HandleWanderingState()
        {
            if (Path == null) // По какой то причине путь был удален Navigator'ом
            {
                CurrentPathPointIndex = 0;
                State = UnitState.Idle;
            }
            else
            {
                CurrentPathPointIndex++;
                if (Path.Count <= CurrentPathPointIndex) // Дошел до конца пути
                {
                    Path = null;
                    CurrentPathPointIndex = 0;
                    State = UnitState.Idle;
                }
                else
                {
                    Field.Unoccupy(transform.position);
                    transform.position = Path[CurrentPathPointIndex];
                    Field.Occupy(transform.position);
                }
            }
        }

        private void HandleIdleState()
        {
            Path = null;
            CurrentPathPointIndex = 0;
            if (Navigator.TryRequestPathToRandomPoint(this))
            {
                State = UnitState.Wandering;
            }
        }

        public void EnterRushingState()
        {
            Path = null;
            CurrentPathPointIndex = 0;

            State = UnitState.RushingToMarkedCell;
        }

        public void ExitRushingState()
        {
            Path = null;
            CurrentPathPointIndex = 0;

            State = UnitState.Idle;
        }

        public void EnterIdleState()
        {
            CurrentPathPointIndex = 0;
            Path = null;
        }
    }
}
