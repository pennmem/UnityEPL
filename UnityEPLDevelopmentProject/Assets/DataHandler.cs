using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//make this a superclass or make a drop down of reporting options?
public class DataHandler : MonoBehaviour 
{
	public DataReporter[] reportersToHandle;

	void Update ()
	{
		foreach (DataReporter reporter in reportersToHandle) 
		{
			if (reporter.UnreadDataPointCount () > 0) 
			{
				DataPoint[] newPoints = reporter.ReadDataPoints (reporter.UnreadDataPointCount ());
				foreach (DataPoint dataPoint in newPoints)
				{
					Debug.Log (dataPoint.ToJSON ());
				}
			}
		}
	}
}