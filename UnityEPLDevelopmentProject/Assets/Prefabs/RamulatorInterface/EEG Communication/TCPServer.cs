using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;

using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using LitJson;

public class TCPServer : MonoBehaviour
{
    Experiment exp { get { return Experiment.Instance; } }


    ThreadedServer myServer;
    public bool isConnected { get { return GetIsConnected(); } }
    public bool canStartGame { get { return GetCanStartGame(); } }
	public bool isPaired { get { return GetIsPaired (); } }
    SubscriberSocket myList;


    //int QUEUE_SIZE = 20;  //Blocks if the queue is full



    //SINGLETON
    private static TCPServer _instance;

    public static TCPServer Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {

        if (_instance != null)
        {
            UnityEngine.Debug.Log("Instance already exists!");
            Destroy(transform.gameObject);
            return;
        }
        _instance = this;

    }

    void Start()
    {
        if (Config.isSYS3)
        {
            RunServer();
        }
    }

    //test clock alignment, every x seconds
    IEnumerator AlignClocks()
    {
        yield return new WaitForSeconds(TCP_Config.numSecondsBeforeAlignment);
        while (true)
        {
            myServer.RequestClockAlignment();
            yield return new WaitForSeconds(TCP_Config.ClockAlignInterval);
        }
    }

    /*//test encoding phase, every x seconds
	IEnumerator SendPhase(bool value){
		yield return new WaitForSeconds(TCP_Config.numSecondsBeforeAlignment);
		while(true){
			myServer.SendStateEvent(GameClock.SystemTime_Milliseconds, "ENCODING", value);
			yield return new WaitForSeconds(10.0f);
		}
	}*/

    void RunServer()
    {
        myServer = new ThreadedServer();
        myServer.Start();
    }

    bool startedAlignClocks = false;
    void Update()
    {
        if (myServer != null)
        {
			if (isConnected && isPaired && !startedAlignClocks)
            {
                startedAlignClocks = true;
                StartCoroutine(AlignClocks());
                myServer.SendInitMessages();
            }
            //			string message = "";
            //			string rcvd;
            //
            //			//	Debug.Log("Update");
            //
            //			while (myList.TryReceiveFrameString(out rcvd))
            //			{
            //				message = message + " - " + rcvd;
            //				UnityEngine.Debug.Log(rcvd);
            //		}
            //DEBUGGING

            //		if (Input.GetKeyDown (KeyCode.A)) {
            //			myServer.isServerConnected = true;
            //		}
            //		if (Input.GetKeyDown (KeyCode.S)) {
            //			myServer.canStartGame = true;
            //		}

        }
    }

    public void Log(long time, TCP_Config.EventType eventType)
    {
        exp.eegLog.Log(time, exp.eegLog.GetFrameCount(), eventType.ToString());
    }

    public void SetState(TCP_Config.DefineStates state, bool isEnabled)
    {
        if (myServer != null)
        {
            if (myServer.isServerConnected)
            {
                myServer.SendStateEvent(GameClock.SystemTime_Milliseconds, state.ToString(), isEnabled);
                UnityEngine.Debug.Log("SET THE STATE FOR BIO-M FILE: " + state.ToString() + isEnabled.ToString());
            }
        }
    }

	public void SetStateWithNum(TCP_Config.DefineStates state, bool isEnabled,int num)
	{
		if (myServer != null)
		{
			if (myServer.isServerConnected)
			{
				myServer.SendStateEventWithNum(GameClock.SystemTime_Milliseconds, state.ToString(), isEnabled,num);
				UnityEngine.Debug.Log("SET THE STATE FOR BIO-M FILE: " + state.ToString() + isEnabled.ToString() + num.ToString());
			}
		}
	}

    public void SendTrialNum(int trialNum)
    {
        if (myServer != null)
        {
            if (myServer.isServerConnected)
            {
				string data = "{\"trial\":" + trialNum.ToString () + "}";
				myServer.SendTrialEvent (GameClock.SystemTime_Milliseconds, TCP_Config.EventType.TRIAL, null, trialNum.ToString());
//				myServer.SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.TRIAL, null,data);
            }
        }
    }

    bool GetIsConnected()
    {
        return myServer.isServerConnected;
    }

    bool GetCanStartGame()
    {
        return myServer.canStartGame;
    }
	bool GetIsPaired()
	{
		return myServer.isPaired;
	}

    void OnApplicationQuit()
    {
        if (myServer != null)
        {
            myServer.End();
            UnityEngine.Debug.Log("Ended server.");
        }
    }


}








//THREADED SERVER
public class ThreadedServer : ThreadedJob
{
    public bool isRunning = false;

    public bool isServerConnected = false;
    public bool isSynced = false;
    public bool canStartGame = false;
	public bool isPaired=false;
    Stopwatch clockAlignmentStopwatch;
    //int numClockAlignmentTries = 0;
    //const int timeBetweenClockAlignmentTriesMS = 500;//500; //half a second
    //const int maxNumClockAlignmentTries = 120; //for a total of 60 seconds of attempted alignment
    string serverAppend = "@tcp://";
    string port = ":5556";

    string HostIPAddress = "127.0.0.1";
    string clientAppend = ">tcp://";


	//ZMQPlugin imports
	[DllImport ("ZMQPlugin")]
	private static extern int ZMQConnect(string hostAddress);


	[DllImport ("ZMQPlugin")]
	private static extern IntPtr ZMQReceive();


	[DllImport ("ZMQPlugin")]
	private static extern void ZMQSend(string message,int length);


	[DllImport ("ZMQPlugin")]
	private static extern void ZMQClose();

	public List<string> messagesToSend = new List<string> ();
	string incompleteMessage = "";

    int socketTimeoutMS = 500; // 500 milliseconds will be the time period within which socket messages will be exchanged

    public ThreadedServer()
    {

    }

    protected override void ThreadFunction()
    {
        isRunning = true;
        // Do your threaded task. DON'T use the Unity API here
		while (isRunning) {
			if (!isServerConnected) {
				InitControlPC ();
			}

			//send heartbeat 
			SendHeartbeatPolled();
			//check for messages
			string message = ReceiveMessageBuffer ();
			UnityEngine.Debug.Log ("Received: " + message);
			ProcessJSONMessageBuffer (message);

			//send messages

			if (messagesToSend.Count > 0) {
				UnityEngine.Debug.Log ("we have " + messagesToSend.Count.ToString () + " messages to send");
				for (int i = 0; i < messagesToSend.Count; i++) {
					string messagesToSendCopy = messagesToSend[i];
					int length = messagesToSendCopy.ToCharArray ().Length;
					PrintDebug ("length: " + length);
					ZMQSend (messagesToSendCopy, length);
				}
			}
			messagesToSend.Clear ();
		}
        CleanupConnections();
	}

	void PrintDebug(string message)
	{
		UnityEngine.Debug.Log (message);
	}

	void InitControlPC()
	{
//		string address = "tcp://localhost:8889";
		string address = "tcp://*:" + TCP_Config.ConnectionPort;
		int connectionStatus = ZMQConnect (address);
		if (connectionStatus == 0)
			PrintDebug("CONNECTED!");
		else
			PrintDebug("CANNOT CONNECT");

		isServerConnected = true;
	}

    void TalkToClient()
    {
        try
        {
            /*if(!isSynced){
				if(numClockAlignmentTries < maxNumClockAlignmentTries){
					CheckClockAlignment();
				}
				else{
					//TODO: what to do if the clock never synced?!
				}
			}*/

            //SEND HEARTBEAT
            //			SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds,TCP_Config.EventType.MESSAGE,"CONNECTED");

//            SendHeartbeatPolled();

      

            //UnityEngine.Debug.Log("MAIN LOOP EXECUTED");


        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("Connection Error....." + e.StackTrace);
        }
    }


    public void SendInitMessages()
    {
        //define event
        SendDefineEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.DEFINE, TCP_Config.GetDefineList());

        //send name of this experiment
        SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.EXPNAME, null, TCP_Config.ExpName);

        //send exp version
        SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.VERSION, null, Config.VersionNumber);

        //send exp session
		SendSessionEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.SESSION, TCP_Config.ExpName,Config.VersionNumber,TCP_Config.SubjectName,Experiment.sessionID);
//        SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.VERSION, null, Config_CoinTask.VersionNumber);

        //send subject ID
		SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.SUBJECTID, null, TCP_Config.SubjectName);

        //NO LONGER REQUEST ALIGNMENT HERE. START IENUMERATOR WHEN TASK IS ACTUALLY STARTING
        //align clocks //SHOULD THIS BE FINISHED BEFORE WE START SENDING HEARTBEATS? -- NO
        //RequestClockAlignment();

        //start heartbeat
        StartHeartbeatPoll();

		//send READY message
		UnityEngine.Debug.Log("sending ready message");
		SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.READY, null, null);


        //wait for "STARTED" message to be received
    }


    void CleanupConnections()
    {
        /* clean up */
        
		ZMQClose ();
        isServerConnected = false;
    }

    void CloseServer()
    {
        try
        {
            isServerConnected = false;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("Close Server Error....." + e.StackTrace);
        }
    }

    //CLOCK ALIGNMENT!
    /*
        Task computer starts the process by sending "ALIGNCLOCK' request.
        Control PC will send a sequence of SYNC messages which are echoed back to it
        When it is complete, the Control PC will send a SYNCED message, which indicates 
        it has completed the clock alignment and it is safe for task computer to proceed 
        to the next step.
		*/
    public void RequestClockAlignment()
    {

        clockAlignmentStopwatch = new Stopwatch();

        isSynced = false;

        SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.ALIGNCLOCK, null, "");
        //SendSimpleJSONEvent(0, TCP_Config.EventType.ALIGNCLOCK, "0", ""); //JUST FOR DEBUGGING
        UnityEngine.Debug.Log("REQUESTING ALIGN CLOCK");

        clockAlignmentStopwatch.Start();
        //numClockAlignmentTries = 0;

    }

    //after x seconds have passed, check if the clocks are aligned yet
    /*int CheckClockAlignment(){
		if(clockAlignmentStopwatch.ElapsedMilliseconds >= timeBetweenClockAlignmentTriesMS){
			if(isSynced){
				UnityEngine.Debug.Log("Sync Complete");
				clockAlignmentStopwatch.Reset();
				return 0;
			}
			else{ //if not synced yet, wait another .5 seconds
				numClockAlignmentTries++;
				clockAlignmentStopwatch.Reset();
				clockAlignmentStopwatch.Start();
				return -1;
			}
		}
		return -1;
	}*/





    //MESSAGE SENDING AND RECEIVING

    //send all "messages to send"
//    void SendMessages()
//    {
//        if (messagesToSend != "")
//        {
//            string messagesToSendCopy = messagesToSend;
//            //			UnityEngine.Debug.Log("SENDING MESSAGE: " + messagesToSendCopy);
//            SendMessage(messagesToSendCopy);
//            if (messagesToSend == messagesToSendCopy)
//            {
//                messagesToSend = "";
//            }
//            else
//            {
//                //				UnityEngine.Debug.Log("CLEARED SENT PART OF MESSAGES TO SEND");
//                messagesToSend = messagesToSend.Substring(messagesToSendCopy.Length);
//            }
//        }
//    }

    //send a single message. don't call this on it's own.
    //should use other methods (EchoMessage, SendEvent, etc.) to add messages to "messagesToSend"
    void SendMessage(string message)
    {
//			ZMQSend(message);
    }

    void EchoMessage(string message)
    {
        ////		messagesToSend += ("ECHO: " + message);
        //		messagesToSend+=JsonMessageController.FormatSimpleJSONEvent (GameClock.SystemTime_Milliseconds,"MESSAGE","CONNECTED");
    }

	public string SendTrialEvent(long systemTime, TCP_Config.EventType eventType, string auxNumber, string eventData)
	{
		string jsonEventString = JsonMessageController.FormatJSONTrialEvent(systemTime, eventType.ToString(), auxNumber, eventData);

		//		UnityEngine.Debug.Log (jsonEventString);

		messagesToSend.Add(jsonEventString);

		return jsonEventString;
	}

    public string SendSimpleJSONEvent(long systemTime, TCP_Config.EventType eventType, string auxNumber, string eventData)
    {

        string jsonEventString = JsonMessageController.FormatSimpleJSONEvent(systemTime, eventType.ToString(), auxNumber, eventData);

        //		UnityEngine.Debug.Log (jsonEventString);

		messagesToSend.Add(jsonEventString);

        return jsonEventString;
    }

    public string SendSimpleJSONEvent(long systemTime, TCP_Config.EventType eventType, string auxNumber, long eventData)
    {

        string jsonEventString = JsonMessageController.FormatSimpleJSONEvent(systemTime, eventType.ToString(), auxNumber.ToString(), eventData);

        UnityEngine.Debug.Log(jsonEventString);

		messagesToSend.Add(jsonEventString);

        return jsonEventString;
    }

	public string SendSessionEvent(long systemTime, TCP_Config.EventType eventType, string experimentName, string expVersion, string subjectID, int sessionNum)
    {

		string jsonEventString = JsonMessageController.FormatJSONSessionEvent(systemTime,experimentName, expVersion, subjectID, sessionNum);

        UnityEngine.Debug.Log(jsonEventString);

		messagesToSend.Add(jsonEventString);

        return jsonEventString;
    }

    public string SendDefineEvent(long systemTime, TCP_Config.EventType eventType, List<string> stateList)
    {

        string jsonEventString = JsonMessageController.FormatJSONDefineEvent(systemTime, stateList);

        UnityEngine.Debug.Log(jsonEventString);

		messagesToSend.Add(jsonEventString);

        return jsonEventString;
    }
	public string SendStateEventWithNum(long systemTime, string stateName, bool value, int number)
	{

		string jsonEventString = JsonMessageController.FormatJSONStateEvent(systemTime, stateName, value,number);

		UnityEngine.Debug.Log(jsonEventString);

		messagesToSend.Add(jsonEventString);

		return jsonEventString;
	}
    public string SendStateEvent(long systemTime, string stateName, bool value)
    {

        string jsonEventString = JsonMessageController.FormatJSONStateEvent(systemTime, stateName, value);

        UnityEngine.Debug.Log(jsonEventString);

		messagesToSend.Add(jsonEventString);

        return jsonEventString;
    }

    void CheckForMessages()
    {
        String message = ReceiveMessageBuffer();

        ProcessJSONMessageBuffer(message);
    }

    string ReceiveMessageBuffer()
    {
        string messageBuffer = "";
        string message = "";
		string receivedMessage = Marshal.PtrToStringAnsi(ZMQReceive());
		if(receivedMessage!="none")
			{
				PrintDebug("received: " + receivedMessage);
			message = receivedMessage;
			}
        messageBuffer = message;

        return messageBuffer;
    }

    //CURRENTLY ASSUMING MESSAGES AREN'T GETTING SPLIT IN HALF.
    public void ProcessJSONMessageBuffer(string messageBuffer)
    {
        //		UnityEngine.Debug.Log ("about to start processsing");
        if (messageBuffer != "")
        {

            char[] individualCharacters = messageBuffer.ToCharArray();
//            UnityEngine.Debug.Log("Processing buffer");
            int numOpenCharacter = 0;
            int numCloseCharacter = 0;
            string message = "";
            for (int i = 0; i < individualCharacters.Length; i++)
            {
                if (incompleteMessage != "")
                {
                    numOpenCharacter = incompleteMessage.Split(TCP_Config.MSG_START).Length - 1;
                    numCloseCharacter = incompleteMessage.Split(TCP_Config.MSG_END).Length - 1;
                }

                if (individualCharacters[i] == TCP_Config.MSG_START)
                {
                    numOpenCharacter++;
                }
                else if (individualCharacters[i] == TCP_Config.MSG_END && numOpenCharacter > numCloseCharacter)
                { //close character should never come before open character(s)
                    numCloseCharacter++;
                }

                message += individualCharacters[i].ToString();

                if (numOpenCharacter == numCloseCharacter && numOpenCharacter > 0)
                { //END OF MESSAGE!
                    UnityEngine.Debug.Log("DECODE MESSAGE: " + message);
                    DecodeJSONMessage(message);

                    //reset variables
                    message = "";

                    numOpenCharacter = 0;
                    numCloseCharacter = 0;
                }
                //if we're on the last character and num open != num close, we have an incomplete message!
                else if (i == individualCharacters.Length - 1 && numOpenCharacter > 0)
                {
                    incompleteMessage = message;
                    UnityEngine.Debug.Log("INCOMPLETE MESSAGE: " + incompleteMessage);
                }

            }
        }
    }


    public void DecodeJSONMessage(string jsonMessage)
    {

        string dataContent = "";
        int dataContentInt = 0;
        string typeContent = "";

        JsonData messageData = JsonMapper.ToObject(jsonMessage);

        typeContent = (string)messageData["type"];
//        UnityEngine.Debug.Log("Type of content is: " + typeContent);
        switch (typeContent)
        {
            case "SUBJECTID":
                //do nothing
                break;

            case "EXPNAME":
                //do nothing
                break;

            case "VERSION":
                //do nothing
                break;

		case "START":
			UnityEngine.Debug.Log ("RECEIVED START MESSAGE");
				canStartGame = true;
				break;
            
		case "CONNECTED":
			UnityEngine.Debug.Log ("connected received");
				isPaired = true;
                break;

            case "SESSION":
                break;

            case "TRIAL":
                dataContentInt = (int)messageData["data"];
                break;

            case "DEFINE":
                break;

            case "STATE":
                break;

            case "HEARTBEAT":
                //do nothing
                break;

            case "ALIGNCLOCK":
                //do nothing
                break;

            case "ABORT":
                //TODO: show message
//                Application.Quit();
                break;

            case "SYNC":
                //Sync received from Control PC
                //Echo SYNC back to Control PC with high precision time so that clocks can be aligned

                //for aux channels 0-9
                for (int i = 0; i < 10; i++)
                {
                    SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.SYNC, i.ToString(), null);
                }
                break;

            case "SYNCED":
                //Control PC is done with clock alignment
                isSynced = true;
                //now align the neuroport if we've received the start message
                //			if(canStartGame){
                //				SendSimpleJSONEvent (GameClock.SystemTime_Milliseconds, TCP_Config.EventType.SYNCNP, "");
                //			}
                break;

            case "EXIT":
                //Control PC is exiting. If heartbeat is active, this is a premature abort.

                /*
                        if self.isHeartbeat and self.abortCallback:
                            self.disconnect()
                            self.abortCallback(self.clock)
                        */

                if (isHeartbeat)
                {
                    //TODO: do this. am I supposed to check for a premature abort? does it matter? or just end it?
                    End();
                }
                //TODO: show message
                UnityEngine.Debug.Log("EXIT happened");
//                Application.Quit();
                break;

            default:
                break;
        }

    }


    //HEARTBEAT
    bool isHeartbeat = false;
    bool hasSentFirstHeartbeat = false;
    long firstBeat = 0;
    long nextBeat = 0;
    long lastBeat = 0;
    long intervalMS = 1000;
    long delta = 0; //is this ever used?

    void StartHeartbeatPoll()
    {
        isHeartbeat = true;
        hasSentFirstHeartbeat = false;
    }

    void StopHeartbeatPoll()
    {
        isHeartbeat = false;
    }

    void SendHeartbeatPolled()
    {
        //Send continuous heartbeat events every 'intervalMillis'
        //The computation assures that the average interval between heartbeats will be intervalMillis rather...
        //...than intervalMillis + some amount of computational overhead because it is relative to a fixed t0.

        if (hasSentFirstHeartbeat)
        {
            long t1 = GameClock.SystemTime_Milliseconds;
			UnityEngine.Debug.Log ("t1: " + t1.ToString () + " firstbeat: " + firstBeat.ToString () + " nextbeat: " + nextBeat.ToString ());
            if ((t1 - firstBeat) > nextBeat)
            {
                //				UnityEngine.Debug.Log("HI HEARTBEAT");
                nextBeat = nextBeat + intervalMS;
                delta = t1 - lastBeat;
                lastBeat = t1;
				UnityEngine.Debug.Log ("Sending heartbeat");
                SendSimpleJSONEvent(lastBeat, TCP_Config.EventType.HEARTBEAT, null, intervalMS.ToString());

                //				EchoMessage ("CONNECTED");
            }
        }
        else
        {
            UnityEngine.Debug.Log("HI FIRST HEARTBEAT");
            firstBeat = GameClock.SystemTime_Milliseconds;
            lastBeat = firstBeat;
            nextBeat = intervalMS;
            SendSimpleJSONEvent(lastBeat, TCP_Config.EventType.HEARTBEAT, null, intervalMS.ToString());
            hasSentFirstHeartbeat = true;
        }
    }



    //FINISHING/ENDING THE THREAD
    protected override void OnFinished()
    {
        // This is executed by the Unity main thread when the job is finished

    }

    public void End()
    {
        if (isServerConnected)
        {
            CloseServer();
        }
        isRunning = false;
    }
}