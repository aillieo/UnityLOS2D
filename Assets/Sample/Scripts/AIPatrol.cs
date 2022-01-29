using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Sample
{
    public class AIPatrol : MonoBehaviour
    {
        [SerializeField]
        private float range = 8f;
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

            if (nmAgent.remainingDistance < 1)
            {
                Vector3 random = new Vector3(
                    Random.Range(-range, range),
                    lastDest.y,
                    Random.Range(-range, range));

                lastDest = random;
                nmAgent.SetDestination(lastDest);
            }
        }
    }
}
