using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDataReporter : DataReporter
{
	public void LogUIEvent(string name)
	{
		eventQueue.Enqueue(new DataPoint(name, RealWorldFrameDisplayTime(), new Dictionary<string, string>()));
	}
}