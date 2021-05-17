using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using NetMQ;
using Newtonsoft.Json.Linq;

public class RamulatorWrapper : IHostPC {
    InterfaceManager manager;

    public RamulatorWrapper(InterfaceManager _manager) {
       manager = _manager; 
       manager.ramulator.manager = manager;
       Start();
       Do(new EventBase(Connect));
    } 

    public override void Connect() {
        manager.ramulator.StartCoroutine(manager.ramulator.BeginNewSession(manager.GetSetting("session")));
        // CoroutineToEvent.StartCoroutine(manager.ramulator.BeginNewSession(manager.GetSetting("session")), manager);
    }

    public override void SendMessage(string type, Dictionary<string, object> data) {
        DataPoint point = new DataPoint(type, DataReporter.TimeStamp(), data);
        string message = point.ToJSON();
        manager.ramulator.SendMessageToRamulator(message);
    }

    public override void HandleMessage(NetMsg msg) {
        throw new NotImplementedException("Ramulator does not expect responses to be handled externally");
    }
    public override JObject SendAndWait(string type, Dictionary<string, object> data, 
                                        string response, int timeout) {
        throw new NotImplementedException("Ramulator does not expect responses to be handled externally");
    }

    public override void Disconnect() {
        manager.ramulator.Disconnect();
        // manager.Do(new EventBase(() => manager.ramulator.Disconnect()));
    }
}

public class RamulatorInterface : MonoBehaviour
{
    //how long to wait for ramulator to connect
    const int timeoutDelay = 150;
    const int unreceivedHeartbeatsToQuit = 8;

    private int unreceivedHeartbeats = 0;

    private NetMQ.Sockets.PairSocket zmqSocket;
    private const string address = "tcp://*:8889";

    public InterfaceManager manager;

    void OnApplicationQuit()
    {
        Disconnect();
    }
    
    public void Disconnect() {
        StopAllCoroutines();

        if (zmqSocket != null) {
            zmqSocket.Close();
            NetMQConfig.Cleanup();
            zmqSocket = null;
        }
    }

    public IEnumerator BeginNewSession(int sessionNumber)
    {
        //Connect to ramulator///////////////////////////////////////////////////////////////////
        zmqSocket = new NetMQ.Sockets.PairSocket();
        zmqSocket.Bind(address);
        //Debug.Log ("socket bound");

        yield return WaitForMessage("CONNECTED", "Ramulated not connected.");

        //SendSessionEvent//////////////////////////////////////////////////////////////////////
        System.Collections.Generic.Dictionary<string, object> sessionData = new Dictionary<string, object>();
        sessionData.Add("name", manager.GetSetting("experimentName"));
        sessionData.Add("version", Application.version);
        sessionData.Add("subject", manager.GetSetting("participantCode"));
        sessionData.Add("session_number", sessionNumber.ToString());
        DataPoint sessionDataPoint = new DataPoint("SESSION", DataReporter.TimeStamp(), sessionData);
        SendMessageToRamulator(sessionDataPoint.ToJSON());
        yield return null;

        //Begin Heartbeats///////////////////////////////////////////////////////////////////////
        InvokeRepeating("SendHeartbeat", 0, 1);

        //SendReadyEvent////////////////////////////////////////////////////////////////////
        DataPoint ready = new DataPoint("READY", DataReporter.TimeStamp(), new Dictionary<string, object>());
        SendMessageToRamulator(ready.ToJSON());
        yield return null;

        yield return WaitForMessage("START", "Start signal not received");

        InvokeRepeating("ReceiveHeartbeat", 0, 1);
    }

    private IEnumerator WaitForMessage(string containingString, string errorMessage)
    {
        string receivedMessage = "";
        float startTime = Time.time;
        while (receivedMessage == null || !receivedMessage.Contains(containingString))
        {
            zmqSocket.TryReceiveFrameString(out receivedMessage);
            if (receivedMessage != "" && receivedMessage != null)
            {
                string messageString = receivedMessage.ToString();
                Debug.Log("received: " + messageString);
                ReportMessage(messageString, false);
            }

            //if we have exceeded the timeout time, show warning and stop trying to connect
            if (Time.time > startTime + timeoutDelay)
            {
                yield break;
            }
            yield return null;
        }
    }

    //ramulator expects this before the beginning of a new list
    public void BeginNewTrial(int trialNumber)
    {
        if (zmqSocket == null)
            throw new Exception("Please begin a session before beginning trials");
        System.Collections.Generic.Dictionary<string, object> sessionData = new Dictionary<string, object>();
        sessionData.Add("trial", trialNumber.ToString());
        DataPoint sessionDataPoint = new DataPoint("TRIAL", DataReporter.TimeStamp(), sessionData);
        SendMessageToRamulator(sessionDataPoint.ToJSON());
    }

    //ramulator expects this when you display words to the subject.
    //for words, stateName is "WORD"
    public void SetState(string stateName, bool stateToggle, System.Collections.Generic.Dictionary<string, object> sessionData)
    {
        sessionData.Add("name", stateName);
        sessionData.Add("value", stateToggle.ToString());
        DataPoint sessionDataPoint = new DataPoint("STATE", DataReporter.TimeStamp(), sessionData);
        SendMessageToRamulator(sessionDataPoint.ToJSON());
    }

    public void SendMathMessage(string problem, string response, int responseTimeMs, bool correct)
    {
        Dictionary<string, object> mathData = new Dictionary<string, object>();
        mathData.Add("problem", problem);
        mathData.Add("response", response);
        mathData.Add("response_time_ms", responseTimeMs.ToString());
        mathData.Add("correct", correct.ToString());
        DataPoint mathDataPoint = new DataPoint("MATH", DataReporter.TimeStamp(), mathData);
        SendMessageToRamulator(mathDataPoint.ToJSON());
    }


    private void SendHeartbeat()
    {
        DataPoint sessionDataPoint = new DataPoint("HEARTBEAT", DataReporter.TimeStamp(), null);
        SendMessageToRamulator(sessionDataPoint.ToJSON());
    }

    private void ReceiveHeartbeat()
    {
        unreceivedHeartbeats = unreceivedHeartbeats + 1;
        Debug.Log("Unreceived heartbeats: " + unreceivedHeartbeats.ToString());

        if (unreceivedHeartbeats > unreceivedHeartbeatsToQuit)
        {
            CancelInvoke("ReceiveHeartbeat");
            CancelInvoke("SendHeartbeat");
            ErrorNotification.Notify(new Exception("Too many missed heartbeats."));
        }

        string receivedMessage = "";
        float startTime = Time.time;
        zmqSocket.TryReceiveFrameString(out receivedMessage);
        if (receivedMessage != "" && receivedMessage != null)
        {
            string messageString = receivedMessage.ToString();
            Debug.Log("heartbeat received: " + messageString);
            ReportMessage(messageString, false);
            unreceivedHeartbeats = 0;
        }
    }

    public void SendMessageToRamulator(string message)
    {
        bool wouldNotHaveBlocked = zmqSocket.TrySendFrame(message, more: false);
        Debug.Log("Tried to send a message: " + message + " \nWouldNotHaveBlocked: " + wouldNotHaveBlocked.ToString());
        ReportMessage(message, true);
    }

    private void ReportMessage(string message, bool sent)
    {
        Dictionary<string, object> messageDataDict = new Dictionary<string, object>();
        messageDataDict.Add("message", message);
        messageDataDict.Add("sent", sent.ToString());
        manager.ReportEvent("network", messageDataDict);
    }
}