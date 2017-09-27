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
		if (UnityEPL.GetExperimentName () != null) 
		{
			newDataDict.Add ("experiment", UnityEPL.GetExperimentName ());
		}

		type = newType;
		dataDict = newDataDict;
		time = newTime;
	}


	//hacky, unity core functionality doesn't include json serialization of dictionaries yet
	public string ToJSON()
	{
		double unixTimestamp = ConvertToMillisecondsSinceEpoch (time);
		string JSONString = "{\"type\":\""+type+"\",\"data\":{";
		foreach (string key in dataDict.Keys)
		{
			string valueString = dataDict [key];
			double valueNumber;
			bool valueBool;
			if (double.TryParse (valueString, out valueNumber))
				;
			else if (bool.TryParse (valueString, out valueBool))
				valueString = valueString.ToLower ();
			else
				valueString = "\"" + valueString + "\"";
			JSONString = JSONString + "\""+key+"\":" + valueString + ",";
		}
		if (dataDict.Count > 0) JSONString = JSONString.Substring (0, JSONString.Length - 1);
		JSONString = JSONString + "},\"time\":"+unixTimestamp.ToString()+"}";
		return JSONString;
	}

	//unimplemented
	public string ToSQL()
	{
		return "unimplemented";
	}

	//unimlemented
	public string ToCSV()
	{
		return "unimplemented";
	}

	public static double ConvertToMillisecondsSinceEpoch(System.DateTime convertMe)
	{
		double milliseconds = (double)(convertMe.ToUniversalTime ().Subtract (new System.DateTime (1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc))).TotalMilliseconds;
		return milliseconds;
	}
}