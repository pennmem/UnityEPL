using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//make this a superclass or make a drop down of reporting options?
public abstract class DataHandler : MonoBehaviour 
{
	public DataReporter[] reportersToHandle;

	protected virtual void Update ()
	{
		foreach (DataReporter reporter in reportersToHandle) 
		{
			if (reporter.UnreadDataPointCount () > 0) 
			{
				DataPoint[] newPoints = reporter.ReadDataPoints (reporter.UnreadDataPointCount ());
				HandleDataPoints (newPoints);
			}
		}
	}

	protected abstract void HandleDataPoints (DataPoint[] dataPoints);
}