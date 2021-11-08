using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Sample
{
    public class PlayerController : MonoBehaviour
    {
        private NavMeshAgent nmAgent;

        private void OnEnable()
        {
            nmAgent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                if (plane.Raycast(r, out float enter))
                {
                    Vector3 targetPosition = r.GetPoint(enter);
                    nmAgent.SetDestination(targetPosition);
                }
            }
        }
    }
}
