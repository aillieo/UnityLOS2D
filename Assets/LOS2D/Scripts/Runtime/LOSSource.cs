using UnityEngine;

namespace AillieoUtils.LOS2D
{
    public class LOSSource : MonoBehaviour
    {
        [Range(0, 180)]
        public float fov = 60f;

        [Range(0, 1000)]
        public float maxDist = 10f;

        public LayerMask maskForEvent = 1;
        public LayerMask maskForRender = 1;

        public float eyeHeight = 0.5f;

        private void OnEnable()
        {
            LOSManager.Register(this);
        }

        private void OnDisable()
        {
            LOSManager.Unregister(this);
        }
    }
}
