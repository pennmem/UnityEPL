using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using MonoLibUsb;

public class FreiburgSyncbox : MonoBehaviour 
{
    public ScriptedEventReporter scriptedEventReporter;

	private const short FREIBURG_SYNCBOX_VENDOR_ID  = 0x0403;
	private const short FREIBURG_SYNCBOX_PRODUCT_ID = 0x6001;
	private const int FREIBURG_SYNCBOX_TIMEOUT_MS = 500;
	private const int FREIBURG_SYNCBOX_PIN_COUNT = 8;
    private const int FREIBURG_SYNCBOX_ENDPOINT = 2;
    private const int FREIBURG_SYNCBOX_INTERFACE_NUMBER = 0;

    private const float TIME_BETWEEN_PULSES_MIN = 0.8f;
    private const float TIME_BETWEEN_PULSES_MAX = 1.2f;

    private MonoLibUsb.MonoUsbSessionHandle sessionHandle = new MonoUsbSessionHandle();
    private MonoLibUsb.Profile.MonoUsbProfileList profileList = null;
    private MonoLibUsb.Profile.MonoUsbProfile freiburgSyncboxProfile = null;
    private MonoLibUsb.MonoUsbDeviceHandle freiburgSyncboxDeviceHandle = null;

	// Use this for initialization
	void Start ()
	{
        BeginFreiburgSyncSession();
	}

    private void BeginFreiburgSyncSession()
    {
        if (sessionHandle.IsInvalid)
            throw new ExternalException("Failed to initialize context.");

        MonoUsbApi.SetDebug(sessionHandle, 0);

        profileList = new MonoLibUsb.Profile.MonoUsbProfileList();

        // The list is initially empty.
        // Each time refresh is called the list contents are updated. 
        int profileListRefreshResult;
        profileListRefreshResult = profileList.Refresh(sessionHandle);
        if (profileListRefreshResult < 0) throw new ExternalException("Failed to retrieve device list.");
        Debug.Log(profileListRefreshResult.ToString() + " device(s) found.");

        // Iterate through the profile list.
        // If we find the device, write 00000000 to its endpoint 2.
        foreach (MonoLibUsb.Profile.MonoUsbProfile profile in profileList)
        {
            if (profile.DeviceDescriptor.ProductID == FREIBURG_SYNCBOX_PRODUCT_ID && profile.DeviceDescriptor.VendorID == FREIBURG_SYNCBOX_VENDOR_ID)
            {
                freiburgSyncboxProfile = profile;
            }
        }

        if (freiburgSyncboxProfile == null)
            throw new ExternalException("None of the connected USB devices were identified as a Freiburg syncbox.");

        freiburgSyncboxDeviceHandle = new MonoUsbDeviceHandle(freiburgSyncboxProfile.ProfileHandle);
        freiburgSyncboxDeviceHandle = freiburgSyncboxProfile.OpenDeviceHandle();
       
        if (freiburgSyncboxDeviceHandle == null)
            throw new ExternalException("The ftd USB device was found but couldn't be opened");

        StartCoroutine(FreiburgPulse());
    }

    private void EndFreiburgSyncSession()
    {
        //These seem not to be required, and in fact cause crashes.  I'm not sure why.
        //freiburgSyncboxDeviceHandle.Close();
        //freiburgSyncboxProfile.Close ();
        //profileList.Close();
        //sessionHandle.Close();
    }

	private IEnumerator FreiburgPulse()
	{
		while (true)
		{
            yield return new WaitForSeconds (Random.Range (TIME_BETWEEN_PULSES_MIN, TIME_BETWEEN_PULSES_MAX));

            int claimInterfaceResult = MonoUsbApi.ClaimInterface(freiburgSyncboxDeviceHandle, FREIBURG_SYNCBOX_INTERFACE_NUMBER);
            int actual_length;
            int bulkTransferResult = MonoUsbApi.BulkTransfer(freiburgSyncboxDeviceHandle, FREIBURG_SYNCBOX_ENDPOINT, byte.MinValue, FREIBURG_SYNCBOX_PIN_COUNT / 8, out actual_length, FREIBURG_SYNCBOX_TIMEOUT_MS);
            if (bulkTransferResult == 0)
                LogPulse();
            Debug.Log("Sync pulse. " + actual_length.ToString() + " byte(s) written.");

            MonoUsbApi.ReleaseInterface(freiburgSyncboxDeviceHandle, FREIBURG_SYNCBOX_INTERFACE_NUMBER);

            if (claimInterfaceResult != 0 || bulkTransferResult != 0)
                break;
		}

        Debug.Log("Restarting sync session.");
        EndFreiburgSyncSession();
        BeginFreiburgSyncSession();
	}

    private void LogPulse()
    {
        scriptedEventReporter.ReportScriptedEvent("Sync pulse begin", new System.Collections.Generic.Dictionary<string, object>());
    }
}