using UnityEngine;

namespace AillieoUtils.LOS2D
{
    [RequireComponent(typeof(Collider))]
    public class LOSTarget : MonoBehaviour
    {
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
