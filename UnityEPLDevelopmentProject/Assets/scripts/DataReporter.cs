using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this superclass implements an interface for retrieving behavioral events from a queue
public abstract class DataReporter : MonoBehaviour 
{
	private static System.DateTime realWorldStartTime = System.DateTime.UtcNow;
	protected System.Collections.Generic.Queue<DataPoint> eventQueue = new Queue<DataPoint>();

	void Awake()
	{
		if (QualitySettings.vSyncCount == 0)
			Debug.LogWarning ("vSync is off!  This will cause tearing, which will prevent meaningful reporting of frame-based time data.");
	}
 
	public int UnreadDataPointCount()
	{
		return eventQueue.Count;
	}

	//UnityEPL users can use this to pull data points out manually on a per-reporter basis
	public DataPoint[] ReadDataPoints(int count)
	{
		if (eventQueue.Count < count) 
		{
			throw new UnityException ("Not enough data points!  Check UnreadDataPointCount first.");
		}

		DataPoint[] dataPoints = new DataPoint[count];
		for (int i = 0; i < count; i++)
		{
			dataPoints [i] = eventQueue.Dequeue ();
		}

		return dataPoints;
	}

	protected System.DateTime RealWorldFrameDisplayTime()
	{
		return realWorldStartTime.AddSeconds (Time.realtimeSinceStartup);
	}
}