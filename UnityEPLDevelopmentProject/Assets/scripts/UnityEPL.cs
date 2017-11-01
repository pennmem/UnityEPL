using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public static class UnityEPL
{
	private static string[] participants = null;
	private static string experiment = null;
	private static string dataPath = null;
	private static int sessionNumber = -1;

	//iPhones require special DLLImport due to static linkage
	//Add this to other external functions if adding iPhone support
	#if UNITY_IPHONE
	[DllImport ("__Internal")]
	#else
	[DllImport ("UnityEPLNativePlugin")]
	#endif
	public static extern double StartCocoaPlugin ();

	[DllImport ("UnityEPLNativePlugin")]
	public static extern void StopCocoaPlugin ();

	[DllImport ("UnityEPLNativePlugin")]
	public static extern int PopKeyKeycode();

	[DllImport ("UnityEPLNativePlugin")]
	public static extern double PopKeyTimestamp();

	[DllImport ("UnityEPLNativePlugin")]
	public static extern int CountKeyEvents();

	[DllImport ("UnityEPLNativePlugin")]
	public static extern int PopMouseButton();

	[DllImport ("UnityEPLNativePlugin")]
	public static extern double PopMouseTimestamp();

	[DllImport ("UnityEPLNativePlugin")]
	public static extern int CountMouseEvents();

	public static void AddParticipant(string participant_ID)
	{
		string[] new_participants;
		if (participants == null)
		{
			new_participants = new string[] { participant_ID };
		}
		else 
		{
			new_participants = new string[participants.Length + 1];
			for (int i = 0; i < participants.Length; i++)
			{
				new_participants [i] = participants [i];
			}
			new_participants [participants.Length] = participant_ID;
		}
		participants = new_participants;
	}

	public static void ClearParticipants()
	{
		participants = null;
	}

	public static string[] GetParticipants()
	{
		if (participants == null)
			return new string[] { "unspecified_participant" };
		return participants;
	}

	public static void SetExperimentName(string experimentName)
	{
		experiment = experimentName;
	}

	public static string GetExperimentName()
	{
		if (experiment == null)
			return "unspecified_experiment";
		return experiment;
	}
		
	public static double MillisecondsSinceTheEpoch()
	{
		return DataPoint.ConvertToMillisecondsSinceEpoch (DataReporter.RealWorldTime ());
	}

	public static string GetParticipantFolder()
	{
		if (dataPath != null)
		{
			Debug.LogWarning ("You have already set a non-default data path.  Returning that instead.");
			return dataPath;
		}

		string defaultRoot = "";
		if (Application.isEditor)
		{
			defaultRoot = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
		}
		else
		{
			defaultRoot = System.IO.Path.GetFullPath (".");
		}
		defaultRoot = System.IO.Path.Combine(defaultRoot, "UnityEPLData");

		string directory = System.IO.Path.Combine (defaultRoot, UnityEPL.GetExperimentName ());
		directory = System.IO.Path.Combine (directory, string.Join ("", UnityEPL.GetParticipants ()));
		return directory;
	}

	public static string GetDataPath ()
	{
		if (dataPath != null)
			return dataPath;

		string directory = GetParticipantFolder ();
		if (sessionNumber != -1)
			directory = System.IO.Path.Combine (directory, "session_" + sessionNumber.ToString());
		return directory;
	}

	public static void SetDataPath(string newDataPath)
	{
		dataPath = newDataPath;
	}

	public static void SetSessionNumber(int newSessionNumber)
	{
		sessionNumber = newSessionNumber;
	}
}