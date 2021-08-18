using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathSegment))]
public class PathSegmentEditor : Editor
{
    PathSegment pathSegment;

    SerializedProperty draw, resets, physicalEnvironment, start,
            virtualPathColor, physicalPathColor, errorColor, 
            segmentTarget, distanceToTarget, virtualWaypoints, 
            isAutomatic, canChooseAutomatic;

    void OnEnable()
    {
        pathSegment = (PathSegment) target;

        draw = serializedObject.FindProperty("draw");
        resets = serializedObject.FindProperty("resets");
        physicalEnvironment = serializedObject.FindProperty("physicalEnvironment");
        start = serializedObject.FindProperty("start");
        virtualPathColor = serializedObject.FindProperty("virtualPathColor");
        physicalPathColor = serializedObject.FindProperty("physicalPathColor");
        errorColor = serializedObject.FindProperty("errorColor");
        segmentTarget = serializedObject.FindProperty("target");
        distanceToTarget = serializedObject.FindProperty("distanceToTarget");
        virtualWaypoints = serializedObject.FindProperty("virtual_waypoints");
        isAutomatic = serializedObject.FindProperty("isAutomatic");
        canChooseAutomatic = serializedObject.FindProperty("canChooseAutomatic");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //using (new EditorGUI.DisabledScope(!canChooseAutomatic.boolValue))
            EditorGUILayout.PropertyField(isAutomatic);

        EditorGUILayout.PropertyField(resets);
        EditorGUILayout.PropertyField(physicalEnvironment);
        EditorGUILayout.PropertyField(start);
        
        EditorGUILayout.PropertyField(segmentTarget);
        EditorGUILayout.PropertyField(distanceToTarget);


        EditorGUILayout.Space();
        GUILayout.Label("Virtual Waypoints", EditorStyles.boldLabel);
        virtualWaypoints.arraySize = EditorGUILayout.IntField(new GUIContent("Number of waypoints:"), virtualWaypoints.arraySize);

        for (int i = 0; i < virtualWaypoints.arraySize; i++)
        {

            SerializedProperty w = virtualWaypoints.GetArrayElementAtIndex(i);

            w.Next(true);
            w.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(w.boolValue, new GUIContent("Virtual Waypoint " + (i + 1) + ":"));

            if (w.boolValue)
            {
                w.Next(true);
                EditorGUILayout.PropertyField(w); // Transform
                if (i > 0)
                {
                    using (new EditorGUI.DisabledScope(isAutomatic.boolValue))
                    {
                        w.Next(true);
                        //EditorGUILayout.PropertyField(w); // File ID
                        w.Next(true);
                        //EditorGUILayout.PropertyField(w); // Path ID
                        w.Next(true);
                        EditorGUILayout.PropertyField(w, new GUIContent("Rotation Gain")); // Rot gain
                        w.Next(true);
                        EditorGUILayout.PropertyField(w, new GUIContent("Translation Gain")); // Trans gain
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(draw);
        EditorGUILayout.PropertyField(virtualPathColor);
        EditorGUILayout.PropertyField(physicalPathColor);
        EditorGUILayout.PropertyField(errorColor);


        serializedObject.ApplyModifiedProperties();
    }
}
