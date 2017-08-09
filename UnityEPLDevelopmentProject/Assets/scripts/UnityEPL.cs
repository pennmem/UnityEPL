﻿using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public static class UnityEPL
{
	private static string[] participants = null;

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
		return participants;
	}
}