using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public static class UnityEPL
{

	#if UNITY_IPHONE
	// On iOS plugins are statically linked into
	// the executable, so we have to use __Internal as the
	// library name.
	[DllImport ("__Internal")]
	#else
	// Other platforms load plugins dynamically, so pass the name
	// of the plugin's dynamic library.
	[DllImport ("UnityEPLNativePlugin")]
	#endif

	public static extern double StartCocoaPlugin ();
}