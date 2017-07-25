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
		if (GUILayout.Button("Handle all reporters"))
			AssignAllReporters();

		base.OnInspectorGUI ();
	}

	private void AssignAllReporters()
	{
		DataHandler currentTarget = (DataHandler)target;

		currentTarget.reportersToHandle = Resources.FindObjectsOfTypeAll<DataReporter> ();
	}
}