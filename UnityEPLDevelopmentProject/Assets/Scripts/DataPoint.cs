using System.Collections;
using UnityEngine;

//these datapoints represent behavioral events
//data about the event is currently stored in a dictionary
public class DataPoint
{
	public string name
	{ get; }
	public System.DateTime time
	{ get; }
	public System.Collections.Generic.Dictionary<string, string> dataDict
	{ get; }


	public DataPoint(string newName, System.DateTime newTime, System.Collections.Generic.Dictionary<string, string> newDataDict)
	{
		name = newName;
		time = newTime;
		dataDict = newDataDict;
	}


	//hacky
	public string ToJSON()
	{
		string JSONString = "{\"name\":\""+name+"\",\"time\":\""+time.ToString()+"\",\"dataDict\":{";
		foreach (string key in dataDict.Keys)
		{
			JSONString = JSONString + "\""+key+"\":\""+dataDict[key]+"\",";
		}
		JSONString = JSONString + "}}";
		return JSONString;
	}
}