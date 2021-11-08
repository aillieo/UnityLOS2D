using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Sample
{
    public class AIPatrol : MonoBehaviour
    {
        private NavMeshAgent nmAgent;
        private Vector3 lastDest;
        public GameObject target;

        private void OnEnable()
        {
            nmAgent = GetComponent<NavMeshAgent>();
            lastDest = transform.position;
        }

        private void Update()
        {
            if (target != null)
            {
                lastDest = target.transform.position;
                nmAgent.SetDestination(lastDest);
            }

            if (Vector3.SqrMagnitude(transform.position - lastDest) < 1)
            {
                Vector3 random = new Vector3(
                    Random.Range(-10, 10),
                    lastDest.y,
                    Random.Range(-10, 10));

                lastDest = random;
                nmAgent.SetDestination(lastDest);
            }
        }
    }
}
