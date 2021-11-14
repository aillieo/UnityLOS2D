using System.Collections;
using System.Collections.Generic;
using AillieoUtils.LOS2D;
using UnityEngine;
using UnityEngine.AI;

namespace Sample
{
    public class SentryController : MonoBehaviour
    {
        [SerializeField]
        private GameObject alertObj;
        private LOSSource losSource;
        private AIPatrol patrol;
        private NavMeshAgent nmAgent;

        private void OnEnable()
        {
            losSource = GetComponent<LOSSource>();
            patrol = GetComponent<AIPatrol>();
            nmAgent = GetComponent<NavMeshAgent>();
            alertObj.SetActive(false);

            LOSManager.OnEnter += OnDetect;
            LOSManager.OnExit += OnLose;
        }

        private void OnDisable()
        {
            LOSManager.OnEnter -= OnDetect;
            LOSManager.OnExit -= OnLose;
        }

        private void LateUpdate()
        {
            Quaternion lookAt = Quaternion.LookRotation(Camera.main.transform.position);
            Vector3 eulerAngles = lookAt.eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            alertObj.transform.eulerAngles = eulerAngles;
        }

        private void OnDetect(LOSSource source, LOSTarget target)
        {
            if (source == losSource)
            {
                this.alertObj.SetActive(true);
                this.patrol.target = target.gameObject;
                this.nmAgent.speed = 1;
            }
        }

        private void OnLose(LOSSource source, LOSTarget target)
        {
            if (source == losSource)
            {
                this.patrol.target = null;
                this.alertObj.SetActive(false);
                this.nmAgent.speed = 0.5f;
            }
        }
    }
}
