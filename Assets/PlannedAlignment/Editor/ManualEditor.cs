using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(Manual))]
public class ManualEditor : Editor
{
	Manual manual;
	
	SerializedProperty virtual_path_segments;
	SerializedProperty physical_range_x;
	SerializedProperty physical_range_y;
	SerializedProperty user_start;
	SerializedProperty resets;
	
	void OnEnable()
	{
		manual = (Manual) target;
		
		virtual_path_segments = serializedObject.FindProperty("virtual_path_segments");
		physical_range_x = serializedObject.FindProperty("physical_range_x");
		physical_range_y = serializedObject.FindProperty("physical_range_y");
		user_start = serializedObject.FindProperty("user_start");
		resets = serializedObject.FindProperty("resets");

	}
	
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		
		GUILayout.Label("Physical:", EditorStyles.boldLabel);
		
		EditorGUILayout.PropertyField(physical_range_x);
		
		EditorGUILayout.PropertyField(physical_range_y);
		
		EditorGUILayout.PropertyField(user_start, new GUIContent("User Starting Transform"));
		
		EditorGUILayout.PropertyField(resets);
		
		EditorGUILayout.Space();
		
		GUILayout.Label("Virtual Path Segments", EditorStyles.boldLabel);
		
		if (GUILayout.Button("Add Path Segment"))
		{
		    manual.AddPathSegment(virtual_path_segments.arraySize);
		}
		
		if (virtual_path_segments.arraySize == 0)
		{
			manual.AddPathSegment(0);
		}
		
		for (int i=0; i<virtual_path_segments.arraySize; i++)
		{
			GUILayout.BeginHorizontal();
			
			EditorGUILayout.PropertyField(virtual_path_segments.GetArrayElementAtIndex(i), new GUIContent("Path Segment " + (i + 1)));
			if (GUILayout.Button(new GUIContent('\u25B2'.ToString(), "Move path segment down"), GUILayout.Width(25)))
			{
				if (i > 0)
				    manual.SwapPathSegments(i, i-1);
			}
			if (GUILayout.Button(new GUIContent('\u25BC'.ToString(), "Move path segment up"), GUILayout.Width(25)))
			{
				if (i < virtual_path_segments.arraySize-2)
    				manual.SwapPathSegments(i, i+1);
			}
			if (GUILayout.Button(new GUIContent('\u2716'.ToString(), "Remove path segment"), GUILayout.Width(25)))
			{
				manual.RemovePathSegment(i);
			}
			
			GUILayout.EndHorizontal();
			
			//EditorGUILayout.Space();
		}
		
		serializedObject.ApplyModifiedProperties();
		
	}
}


