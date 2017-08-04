using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public static class UnityEPL
{

	//iPhones require special DLLImport due to static linkage
	//Add this to other external functions if adding iPhone support
	#if UNITY_IPHONE
	[DllImport ("__Internal")]
	#else
	[DllImport ("UnityEPLNativePlugin")]
	#endif
	public static extern double StartCocoaPlugin ();

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
}