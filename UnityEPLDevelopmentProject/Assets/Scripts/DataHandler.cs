using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//make this a superclass or make a drop down of reporting options?
public class DataHandler : MonoBehaviour 
{
	public DataReporter[] reportersToHandle;

	public UnityEngine.UI.Text debugText;

	void Update ()
	{
		foreach (DataReporter reporter in reportersToHandle) 
		{
			if (reporter.UnreadDataPointCount () > 0) 
			{
				DataPoint[] newPoints = reporter.ReadDataPoints (reporter.UnreadDataPointCount ());
				foreach (DataPoint dataPoint in newPoints)
				{
					debugText.text = dataPoint.ToJSON();
				}
			}
		}
	}
}