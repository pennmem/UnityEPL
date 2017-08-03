using System.Collections;
using UnityEngine;

//these datapoints represent behavioral events
//data about the event is currently stored in a dictionary
public class DataPoint
{
	public string type
	{ get; }
	public System.Collections.Generic.Dictionary<string, string> dataDict
	{ get; }
	public System.DateTime time
	{ get; }

//  back-of-the-envelope test
	static private double last_milliseconds = 0;
	static private double sum = 0;
	static private int count = 0;

	public DataPoint(string newType, System.DateTime newTime, System.Collections.Generic.Dictionary<string, string> newDataDict)
	{
		type = newType;
		dataDict = newDataDict;
		time = newTime;
	}


	//hacky
	public string ToJSON()
	{
		double unixTimestamp = TimeInMilliseconds ();
		string JSONString = "{\"type\":\""+type+"\",\"dataDict\":{";
		foreach (string key in dataDict.Keys)
		{
			JSONString = JSONString + "\""+key+"\":\""+dataDict[key]+"\",";
		}
		if (dataDict.Count > 0) JSONString = JSONString.Substring (0, JSONString.Length - 1);
		JSONString = JSONString + "},\"time\":\""+unixTimestamp.ToString()+"\"}";
		return JSONString;
	}

	private double TimeInMilliseconds()
	{
		double milliseconds = (double)(time.ToUniversalTime ().Subtract (new System.DateTime (1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc))).TotalMilliseconds;
//		back-of-the-envelope test
		if (last_milliseconds == 0)
			last_milliseconds = milliseconds - 17;
		double difference = milliseconds - last_milliseconds;
		Debug.Log ("difference from last: " + difference.ToString ());
		sum += difference;
		last_milliseconds = milliseconds;
		count = count + 1;
		//UnityEngine.Debug.Log (sum / count);
		if (count == 100) 
		{
			count = 0;
			sum = 0;
		}
		return milliseconds;
	}
}