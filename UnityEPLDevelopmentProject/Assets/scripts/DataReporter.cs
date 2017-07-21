using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataReporter : MonoBehaviour 
{
	public string reportingID = "Object ID not set.";
	public bool reportTransform = true;
	public int framesPerTransformReport = 60;
	public bool reportEntersView = true;
	public bool reportLeavesView = true;
	//public 

	void Awake()
	{
		if (QualitySettings.vSyncCount == 0)
			Debug.LogWarning ("vSync is off!  This will cause tearing, which will prevent meaningful reporting of frame-based time data.");
	}
 
	void Start () 
	{
		Debug.Log (UnityEPL.TestNativePluginFunction ());
	}

	void Update () 
	{
		
	}
}