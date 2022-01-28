using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AillieoUtils.LOS2D
{
    [CustomEditor(typeof(LOSMesh))]
    public class LOSMeshEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LOSMesh m = target as LOSMesh;

            SerializedProperty associatedLOSSource = serializedObject.FindProperty(nameof(m.associatedLOSSource));

            if (!Application.isPlaying)
            {
                EditorGUILayout.PropertyField(associatedLOSSource);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("meshComp"));
            }

            GUIStyle labelBold = new GUIStyle("label") { fontStyle = FontStyle.Bold };
            EditorGUILayout.LabelField("Mesh Parameters", labelBold);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m.resolution)));

            if (associatedLOSSource.objectReferenceValue != null)
            {
                LOSSource source = associatedLOSSource.objectReferenceValue as LOSSource;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField("fov", source.fov);
                EditorGUILayout.FloatField("maxDist", source.maxDist);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m.defaultFOV)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m.defaultMaxDist)));
            }

            EditorGUILayout.LabelField("Mesh Display", labelBold);

            SerializedProperty drawSimpleSector = serializedObject.FindProperty(nameof(m.drawSimpleSector));

            EditorGUILayout.PropertyField(drawSimpleSector);

            if (!drawSimpleSector.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m.drawHidden)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m.drawSight)));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m.fillUV)));

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m.autoRegenerateMesh)));

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("RegenerateMesh"))
            {
                (target as LOSMesh).RegenerateMesh();
            }
        }
    }
}
