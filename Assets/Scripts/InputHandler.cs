using System;
using UnityEngine;

namespace NivalTask
{
    public class InputHandler : MonoBehaviour
    {
        public float RaycastMaxDistance = 100f;
        public LayerMask RaycastMask;

        public Field Field;

        private void Update()
        {
            HandleRightMouseButton();
            HandleLeftMouseButton();
        }

        private void HandleLeftMouseButton()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, RaycastMaxDistance, RaycastMask))
                {
                    var go = hit.collider.gameObject;
                    if (go.CompareTag("BlockingCube"))
                    {
                        Field.Unblock(hit.point);
                    }
                    else if (go.CompareTag("Field"))
                    {
                        Field.Block(hit.point);
                    }
                }
            }
        }

        private void HandleRightMouseButton()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, RaycastMaxDistance, RaycastMask))
                {
                    var go = hit.collider.gameObject;
                    if (go.CompareTag("Field"))
                    {
                        Field.Mark(hit.point);
                    }
                }
            }
        }
    }
}
