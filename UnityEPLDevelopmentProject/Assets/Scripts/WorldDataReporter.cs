using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldDataReporter : DataReporter
{
	public string reportingID = "Object ID not set.";
	public bool reportTransform = true;
	public int framesPerTransformReport = 60;
	public bool reportEntersView = true;
	public bool reportLeavesView = true;
 
	void Start () 
	{
		Debug.Log (UnityEPL.TestNativePluginFunction ());
	}

	void Update ()
	{
		if (Time.frameCount % framesPerTransformReport == 0)
		{
			System.Collections.Generic.Dictionary<string, string> transformDict = new System.Collections.Generic.Dictionary<string, string>();
			transformDict.Add ("positionX", transform.position.x.ToString ());
			transformDict.Add ("positionY", transform.position.y.ToString ());
			transformDict.Add ("positionZ", transform.position.z.ToString ());
			transformDict.Add ("rotationX", transform.position.x.ToString ());
			transformDict.Add ("rotationY", transform.position.y.ToString ());
			transformDict.Add ("rotationZ", transform.position.z.ToString ());
			transformDict.Add ("scaleX", transform.position.x.ToString ());
			transformDict.Add ("scaleY", transform.position.y.ToString ());
			transformDict.Add ("scaleZ", transform.position.z.ToString ());
			eventQueue.Enqueue (new DataPoint (gameObject.name + " transform", RealWorldFrameDisplayTime(), transformDict));
		}
	}
}