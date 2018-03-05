using System.Collections;
using UnityEngine;

//these datapoints represent behavioral events
//data about the event is currently stored in a dictionary
public class DataPoint
{
    private string type;
    private System.Collections.Generic.Dictionary<string, object> dataDict;
    private System.DateTime time;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:DataPoint"/> class.  This represents a piece of data that you might want to keep about your project.
    /// 
    /// "Type" is a short description of the data.  Time is the time when the datapoint occured (or was collected, if it's a continuous event).
    /// 
    /// dataDict contains the actual data that you might want to analyze later.  Each element of the data is a key-value pair, the key representing its name.
    /// The value can be any C# object.  If the datapoint is written to disk using a WirteToDiskHandler, the handler will try to deduce an appropriate way of
    /// serializing the object.  This is easy for strings, integers, booleans, etc., but for other objects the object's ToString method might be used as a fallback.
    /// </summary>
    /// <param name="newType">New type.</param>
    /// <param name="newTime">New time.</param>
    /// <param name="newDataDict">New data dict.</param>
    public DataPoint(string newType, System.DateTime newTime, System.Collections.Generic.Dictionary<string, object> newDataDict)
    {
        if (newDataDict == null)
            newDataDict = new System.Collections.Generic.Dictionary<string, object>();
        string[] participants = UnityEPL.GetParticipants();
        if (participants != null)
        {
            for (int i = 0; i < participants.Length; i++)
            {
                newDataDict.Add("participant " + (i + 1).ToString(), participants[i]);
            }
        }
        if (UnityEPL.GetExperimentName() != null)
        {
            newDataDict.Add("experiment", UnityEPL.GetExperimentName());
        }

        type = newType;
        dataDict = newDataDict;
        time = newTime;
    }

    /// <summary>
    /// Returns a JSON string representing this datapoint.
    /// 
    /// Strings conforming to certain formats will be converted to corresponding types.  For example, if a string looks like a number it will be represented as a JSON number type. 
    /// </summary>
    /// <returns>The json.</returns>
    public string ToJSON()
    {
        double unixTimestamp = ConvertToMillisecondsSinceEpoch(time);
        string JSONString = "{\"type\":\"" + type + "\",\"data\":{";
        foreach (string key in dataDict.Keys)
        {
            string value = dataDict[key].ToString();

            string valueJSONString;
            double valueNumber;
            bool valueBool;

            if (value.Length > 2 && value[0].Equals('(') && value[value.Length - 1].Equals(')')) //tuples
            {
                char[] charArray = value.ToCharArray();
                charArray[0] = '[';
                charArray[charArray.Length - 1] = ']';
                if (charArray[charArray.Length - 2].Equals(','))
                    charArray[charArray.Length - 2] = ' ';
                valueJSONString = new string(charArray);
            }
            else if ((value.Length > 1 && value[0].Equals('{') && value[value.Length - 1].Equals('}')) ||
                     (double.TryParse(value, out valueNumber))) //embedded json or numbers
            {
                valueJSONString = value;
            }
            else if (bool.TryParse(value, out valueBool)) //bools
            {
                valueJSONString = value.ToLower();
            }
            else //everything else is a string
            {
                valueJSONString = "\"" + value + "\"";
            }
            JSONString = JSONString + "\"" + key + "\":" + valueJSONString + ",";
        }
        if (dataDict.Count > 0) JSONString = JSONString.Substring(0, JSONString.Length - 1);
        JSONString = JSONString + "},\"time\":" + unixTimestamp.ToString() + "}";
        return JSONString;
    }

    public static double ConvertToMillisecondsSinceEpoch(System.DateTime convertMe)
    {
        double milliseconds = (double)(convertMe.ToUniversalTime().Subtract(new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc))).TotalMilliseconds;
        return milliseconds;
    }
}