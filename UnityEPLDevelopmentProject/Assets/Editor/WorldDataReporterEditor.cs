using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldDataReporter))]
[CanEditMultipleObjects]
public class WorldDataReporterEditor : Editor
{
    void OnEnable()
    {
        WorldDataReporter currentTarget = (WorldDataReporter)target;
        //give the WorldDataReporter a unique ID if none has been set manually
        if (currentTarget.reportingID.Equals("Object ID not set."))
            GenerateDefaultName();
    }

    public override void OnInspectorGUI()
    {
        WorldDataReporter currentTarget = (WorldDataReporter)target;

        //Allow users to set ID manually
        currentTarget.reportingID = EditorGUILayout.TextField("Reporting ID", currentTarget.reportingID);
        //or generate a default one
        if (GUILayout.Button("New unique ID"))
            GenerateDefaultName();

        //provide selections for which data to report, and the frequency
        currentTarget.reportTransform = EditorGUILayout.Toggle("Report transform data", currentTarget.reportTransform);
        if (currentTarget.reportTransform)
        {
            EditorGUI.indentLevel++;
            int input = EditorGUILayout.IntField("Frames per transform report", currentTarget.framesPerTransformReport);
            if (input > 0)
                currentTarget.framesPerTransformReport = input;
            EditorGUI.indentLevel--;
        }

        currentTarget.reportEntersView = EditorGUILayout.Toggle("Report upon entering view", currentTarget.reportEntersView);
        currentTarget.reportLeavesView = EditorGUILayout.Toggle("Report upon leaving view", currentTarget.reportLeavesView);
    }

    private void GenerateDefaultName()
    {
        WorldDataReporter currentTarget = (WorldDataReporter)target;
        currentTarget.reportingID = currentTarget.name + System.Guid.NewGuid();
    }
}