using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataHandler))]
[CanEditMultipleObjects]
public class DataHandlerEditor : Editor 
{
	void OnEnable()
	{
	
	}

	public override void OnInspectorGUI()
	{
		DataHandler currentTarget = (DataHandler)target;

		if (GUILayout.Button("Handle all reporters"))
			currentTarget.reportersToHandle = FindObjectsOfType<DataReporter> ();

		base.OnInspectorGUI ();
	}
}