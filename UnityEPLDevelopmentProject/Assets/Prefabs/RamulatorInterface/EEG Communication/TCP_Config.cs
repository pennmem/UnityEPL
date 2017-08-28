using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TCP_Config : MonoBehaviour {

	public static float numSecondsBeforeAlignment = 10.0f;

	public static string HostIPAddress = "192.168.137.200"; //"169.254.50.2" for Mac Pro Desktop.
	public static int ConnectionPort = 8889; //8001 for Mac Pro Desktop communication


	public static char MSG_START = '{';
	public static char MSG_END = '}';

	public static string ExpName { get { return GetExpName (); } }
	public static string SubjectName = ExperimentSettings.currentSubject.name;

	public static float ClockAlignInterval = 60.0f; //this should happen about once a minute

	public enum EventType {
		SUBJECTID,
		EXPNAME,
		VERSION,
		INFO,
		CONTROL,
		DEFINE,
		SESSION,
		PRACTICE,
		TRIAL,
		PHASE,
		DISPLAYON,
		DISPLAYOFF,
		HEARTBEAT,
		ALIGNCLOCK,
		ABORT,
		SYNC,
		SYNCNP,
		SYNCED,
		READY,
		STATE,
		EXIT
	}

	public enum SessionType{
		CLOSED_STIM,
		OPEN_STIM,
		NO_STIM
	}

	public static SessionType sessionType { get { return GetSessionType (); } }


	void Start(){

	}

	static string GetExpName(){
		return Config.BuildVersion.ToString ();
	}
	
	public static SessionType GetSessionType(){
		switch (Config.BuildVersion) {
		case Config.Version.DBoy1:
			return SessionType.NO_STIM;
		case Config.Version.DBoy2:
			return SessionType.CLOSED_STIM ;//could change back to openstim... just use closedstim for now
		case Config.Version.DBoy3:
			return SessionType.CLOSED_STIM;
		}

		return SessionType.NO_STIM;
	}

	//fill in how you see fit!
	public enum DefineStates
	{
		EYETRACKER_CALIBRATION,
		EYETRACKER_VALIDATION,
		EYETRACKER_RECONNECTION,
		EYETRACKER_RESULTS,
		MIC_TEST,
		INSTRUCTION_VIDEO,
		LEARNING_PRESENTATION_PHASE,
		LEARNING_NAVIGATION_PHASE,
		STORE_TARGET_0,
		STORE_TARGET_1,
		STORE_TARGET_2,
		STORE_TARGET_3,
		STORE_TARGET_4,
		STORE_TARGET_5,
		STORE_TARGET_6,
		STORE_TARGET_7,
		STORE_TARGET_8,
		STORE_TARGET_9,
		STORE_TARGET_10,
		STORE_TARGET_11,
		STORE_TARGET_12,
		ITEM_DELIVERY_0,
		ITEM_DELIVERY_1,
		ITEM_DELIVERY_2,
		ITEM_DELIVERY_3,
		ITEM_DELIVERY_4,
		ITEM_DELIVERY_5,
		ITEM_DELIVERY_6,
		ITEM_DELIVERY_7,
		ITEM_DELIVERY_8,
		ITEM_DELIVERY_9,
		ITEM_DELIVERY_10,
		ITEM_DELIVERY_11, // only 12 item deliveries
		STORE_CUE_0,
		STORE_CUE_1,
		STORE_CUE_2,
		STORE_CUE_3,
		STORE_CUE_4,
		STORE_CUE_5,
		STORE_CUE_6,
		STORE_CUE_7,
		STORE_CUE_8,
		STORE_CUE_9,
		STORE_CUE_10,
		STORE_CUE_11, //only 12 stores delivered to
		ITEM_CUE_0,
		ITEM_CUE_1,
		ITEM_CUE_2,
		ITEM_CUE_3,
		ITEM_CUE_4,
		ITEM_CUE_5,
		ITEM_CUE_6,
		ITEM_CUE_7,
		ITEM_CUE_8,
		ITEM_CUE_9,
		ITEM_CUE_10,
		ITEM_CUE_11, //only 12 items delivered
		DELIVERY_NAVIGATION,
		RECALL_CUED,
		RECALL_FREE_ITEM,
		RECALL_FREE_AND_CUED,
		FINALRECALL_STORE,
		FINALRECALL_ITEM,
		PAUSED
	}

	public static List<string> GetDefineList(){
		List<string> defineList = new List<string> ();

		DefineStates[] values = (DefineStates[])DefineStates.GetValues(typeof(DefineStates));

		foreach (DefineStates defineState in values)
		{
			defineList.Add(defineState.ToString());
		}

		return defineList;
	}

}
