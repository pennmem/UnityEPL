﻿using System.Collections;
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
		if (newDataDict == null)
			newDataDict = new System.Collections.Generic.Dictionary<string, string> ();
		string[] participants = UnityEPL.GetParticipants ();
		if (participants != null) 
		{
			for (int i = 0; i < participants.Length; i++)
			{
				newDataDict.Add ("participant " + (i + 1).ToString (), participants [i]);
			}
		}

		type = newType;
		dataDict = newDataDict;
		time = newTime;
	}


	//hacky, unity core functionality doesn't include json serialization of dictionaries yet
	public string ToJSON()
	{
		double unixTimestamp = UnixTimeInMilliseconds ();
		string JSONString = "{\"type\":\""+type+"\",\"data\":{";
		foreach (string key in dataDict.Keys)
		{
			string value = dataDict [key];
			JSONString = JSONString + "\""+key+"\":\""+value+"\",";
		}
		if (dataDict.Count > 0) JSONString = JSONString.Substring (0, JSONString.Length - 1);
		JSONString = JSONString + "},\"time\":"+unixTimestamp.ToString()+"}";
		return JSONString;
	}

	private double UnixTimeInMilliseconds()
	{
		double milliseconds = (double)(time.ToUniversalTime ().Subtract (new System.DateTime (1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc))).TotalMilliseconds;
		return milliseconds;
	}
}