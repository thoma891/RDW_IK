using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Waypoint))]
public class WaypointEditor : Editor
{
    Waypoint waypoint;
    SerializedProperty transform;
    SerializedProperty gain_rotation;
    SerializedProperty gain_translation;


    void OnEnable()
    {
        //waypoint = (Waypoint) target;

        transform = serializedObject.FindProperty("transform");
        gain_rotation = serializedObject.FindProperty("gain_rotation");
        gain_translation = serializedObject.FindProperty("gain_translation");


    }



    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(transform);
        EditorGUILayout.PropertyField(gain_rotation, new GUIContent("Rotation Gain"));
        EditorGUILayout.PropertyField(gain_translation);

        serializedObject.ApplyModifiedProperties();
    }
}