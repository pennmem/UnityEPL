using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		Debug.Log (UnityEPL.TestNativePluginFunction ());
		gameObject.SetActive (false);
	}

	void Update () 
	{
		
	}
}