using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AillieoUtils.LOS2D
{
    [CreateAssetMenu(fileName = "LOSConfig")]
    public class LOSConfig : ScriptableObject
    {
        public LayerMask LOSObstacle;
    }
}
