using UnityEditor;

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