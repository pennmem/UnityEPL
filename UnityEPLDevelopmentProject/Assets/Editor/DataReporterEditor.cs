using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataReporter))]
[CanEditMultipleObjects]
public class DataReporterEditor : Editor 
{
	void OnEnable()
	{
		DataReporter currentTarget = (DataReporter)target;
		if (currentTarget.reportingID.Equals("Object ID not set."))
			GenerateDefaultName ();
	}

	public override void OnInspectorGUI()
	{
		DataReporter currentTarget = (DataReporter)target;
		currentTarget.reportingID = EditorGUILayout.TextField ("Reporting ID", currentTarget.reportingID);
		if (GUILayout.Button("New unique ID"))
			GenerateDefaultName();
	}

	//give the DataReporter a unique ID if none has been set manually
	private void GenerateDefaultName()
	{
		DataReporter currentTarget = (DataReporter)target;
		currentTarget.reportingID = currentTarget.name + System.Guid.NewGuid();
	}
}