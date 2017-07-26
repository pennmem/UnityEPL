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


	public DataPoint(string newType, System.DateTime newTime, System.Collections.Generic.Dictionary<string, string> newDataDict)
	{
		type = newType;
		dataDict = newDataDict;
		time = newTime;
	}


	//hacky
	public string ToJSON()
	{
		int unixTimestamp = (int)(System.DateTime.UtcNow.ToUniversalTime().Subtract(new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds);
		string JSONString = "{\"type\":\""+type+"\",\"dataDict\":{";
		foreach (string key in dataDict.Keys)
		{
			JSONString = JSONString + "\""+key+"\":\""+dataDict[key]+"\",";
		}
		if (dataDict.Count > 0) JSONString = JSONString.Substring (0, JSONString.Length - 1);
		JSONString = JSONString + "},\"time\":\""+unixTimestamp.ToString()+"\"}";
		return JSONString;
	}
}