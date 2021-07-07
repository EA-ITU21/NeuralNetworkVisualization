using UnityEngine;
using UnityEditor;

namespace NetworkModel
{
    [CustomEditor(typeof(GenomeManager))]
    public class GenomeManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedObject serializedObject = new SerializedObject(target);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("carController"));
            EditorGUILayout.Space(10);

            SerializedProperty runMode = serializedObject.FindProperty("runMode");
            EditorGUILayout.PropertyField(runMode);

            if (((RunMode)runMode.intValue) == RunMode.Test)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("testNetworks"));
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Test Mode References", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("testNetworkSelectionPanel"), new GUIContent("Network Selection Panel"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("testNetworkSelectionButtonPrefab"), new GUIContent("Network Selection Button Prefab"));
            }
            else if (((RunMode)runMode.intValue) == RunMode.Training)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Neuron Count In Each Layer", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("neuronCounts"));
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Population Controls", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("populationCount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mutationRate"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bestAgentSelection"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("worstAgentSelection"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("crossoverCount"));
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Training Mode References", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("episodeInfoPanel"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("generationNumberText"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("episodeNumberText"));
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("saveNetworkButton"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}