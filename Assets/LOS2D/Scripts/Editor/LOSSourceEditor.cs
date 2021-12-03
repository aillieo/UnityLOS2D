using UnityEditor;
using UnityEngine;

namespace AillieoUtils.LOS2D
{
    [CustomEditor(typeof(LOSSource))]
    public class LOSSourceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LOSSource s = target as LOSSource;

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.fov)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.maxDist)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.maskForEvent)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.maskForRender)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.eyeHeight)));

            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("- Targets In Sight");

                foreach (var t in LOSManager.GetTargetsInSight(s))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(t.name);
                    EditorGUILayout.ObjectField(t, typeof(LOSTarget), true);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }
        }
    }
}
