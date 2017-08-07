using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogHandler : DataHandler 
{
	protected override void HandleDataPoints(DataPoint[] dataPoints)
	{
		foreach (DataPoint dataPoint in dataPoints)
			Debug.Log (dataPoint.ToJSON ());
	}
}
