using UnityEditor;
using UnityEngine;

namespace AillieoUtils.LOS2D
{
    [CustomEditor(typeof(LOSTarget))]
    public class LOSTargetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                LOSTarget t = target as LOSTarget;
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("- Seen By Sources");

                foreach (var s in LOSManager.GetSourcesHaveSightOf(t))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(s.name);
                    EditorGUILayout.ObjectField(s, typeof(LOSSource), true);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }
        }
    }
}
