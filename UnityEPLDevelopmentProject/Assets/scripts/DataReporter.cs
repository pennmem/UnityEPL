using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DataReporter : MonoBehaviour 
{
	private string reportingID = "Object ID not set.";

	void Awake()
	{
		if (QualitySettings.vSyncCount == 0)
			Debug.LogWarning ("vSync is off!  This will cause tearing, which will prevent meaningful reporting of frame-based time data.");
	}
 
	void Start () 
	{
		
	}

	void Update () 
	{
		
	}
}

[CustomEditor(typeof(DataReporter))]
[CanEditMultipleObjects]
public class DataReporterEditor : Editor 
{
	SerializedProperty lookAtPoint;

	void OnEnable()
	{
		lookAtPoint = serializedObject.FindProperty("lookAtPoint");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.PropertyField(lookAtPoint);
		serializedObject.ApplyModifiedProperties();
	}
}