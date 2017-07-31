using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class InputReporter : DataReporter
{
	void Awake()
	{
		
	}

	void OnGUI()
	{
		string type = "";
		string buttonName = "";
		Event thisEvent = Event.current;
		if (thisEvent.isKey)
		{
			type = "key press";
			buttonName = thisEvent.keyCode.ToString ();
			if (thisEvent.keyCode == KeyCode.None)
				return;
		}
		if (thisEvent.isMouse)
		{
			type = "mouse button press";
			buttonName = thisEvent.button.ToString ();
		}
		if (thisEvent.isKey || thisEvent.isMouse)
		{
			eventQueue.Enqueue (new DataPoint (type, RealWorldTime (), new Dictionary<string, string> (){ { type , buttonName } }));
		}
	}
}