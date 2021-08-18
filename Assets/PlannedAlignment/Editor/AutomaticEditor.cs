using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(Automatic))]
public class AutomaticEditor : Editor
{
	Automatic automatic;
	
	SerializedProperty virtual_path_segments;
	
	void OnEnable()
	{
		automatic = (Automatic) target;
		
		virtual_path_segments = serializedObject.FindProperty("virtualPathSegments");

	}
	
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		
		
		GUILayout.Label("Virtual Path Segments", EditorStyles.boldLabel);
		
		if (GUILayout.Button("Add Path Segment"))
		{
		    automatic.AddPathSegment(virtual_path_segments.arraySize);
		}
		
		if (virtual_path_segments.arraySize == 0)
		{
			automatic.AddPathSegment(0);
		}
		
		for (int i=0; i<virtual_path_segments.arraySize; i++)
		{
			GUILayout.BeginHorizontal();
			
			EditorGUILayout.PropertyField(virtual_path_segments.GetArrayElementAtIndex(i), new GUIContent("Path Segment " + (i + 1)));
			if (GUILayout.Button(new GUIContent('\u25B2'.ToString(), "Move path segment down"), GUILayout.Width(25)))
			{
				if (i > 0)
				    automatic.SwapPathSegments(i, i-1);
			}
			if (GUILayout.Button(new GUIContent('\u25BC'.ToString(), "Move path segment up"), GUILayout.Width(25)))
			{
				if (i < virtual_path_segments.arraySize-2)
    				automatic.SwapPathSegments(i, i+1);
			}
			if (GUILayout.Button(new GUIContent('\u2716'.ToString(), "Remove path segment"), GUILayout.Width(25)))
			{
				automatic.RemovePathSegment(i);
			}
			
			GUILayout.EndHorizontal();
			
			//EditorGUILayout.Space();
		}
		
		serializedObject.ApplyModifiedProperties();
		
	}
}


