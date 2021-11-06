using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
    public class PlayerController : MonoBehaviour
    {
        public float speed = 1f;
        private Vector3 targetPosition;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                if (plane.Raycast(r, out float enter))
                {
                    targetPosition = r.GetPoint(enter);
                    this.transform.LookAt(targetPosition);
                }
            }

            this.transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
    }

}
