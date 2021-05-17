using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;

public abstract class IHostPC : EventLoop {
    public abstract JObject SendAndWait(string type, Dictionary<string, object> data, 
                                        string response, int timeout);
    public abstract void Connect();
    public abstract void Disconnect();
    public abstract void HandleMessage(NetMsg msg);
    public abstract void SendMessage(string type, Dictionary<string, object> data);
}

public struct NetMsg {
    public string msg;
    public DateTime time;

    public NetMsg(string msg, DateTime time) {
        this.msg = msg;
        this.time = time;
    }
}

public class HostPCListener : MessageHandler<NetMsg> {

    // FIXME: catch socket exceptions to give human readable errors

    Byte[] buffer; 
    const Int32 bufferSize = 2048;

    CancellationTokenSource tokenSource;

    string messageBuffer = "";
    public HostPCListener(IHostPC host) : base(host, host.HandleMessage) {
        buffer = new Byte[bufferSize];
    }

    public bool IsListening() {
        return tokenSource?.IsCancellationRequested ?? false;
    }

    public async Task<NetMsg> Listen(NetworkStream stream) {
        /*
        Asynchronously listens for a single network message and
        passes it to the host message queue through the action
        used to construct this listener.
        */

        if(IsListening()) {
            throw new AccessViolationException("Already Listening");
        }

        if(tokenSource == null) {
            tokenSource = new CancellationTokenSource();
        }

        NetMsg message;
        int bytesRead;

        do {
            bytesRead = await stream.ReadAsync(buffer, 0, bufferSize, tokenSource.Token);
        } while(!ParseBuffer(bytesRead, out message));

        Do(message);

        if(!tokenSource.IsCancellationRequested) {
            _ = Listen(stream);
        }
        else {
            tokenSource.Dispose();
            tokenSource = null;
        }

        return message;
    }

    public void CancelRead() {
        tokenSource?.Cancel();
    }

    private bool ParseBuffer(int bytesRead, out NetMsg msgResult) {
        /*
        Extract a full message from the byte buffer, and leave remaining characters
        in string buffer. Return true if message terminating newline is read from string, 
        false otherwise.
        */

        messageBuffer += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

        string message = messageBuffer.Substring(0, messageBuffer.IndexOf("\n") + 1);
        messageBuffer = messageBuffer.Substring(messageBuffer.IndexOf("\n") + 1);

        msgResult = new NetMsg(message, DataReporter.TimeStamp());

        return message.Length > 0;
    }
}

public class ElememInterface : IHostPC 
{
    public InterfaceManager im;

    int messageTimeout = 1000;
    int heartbeatTimeout = 8000; // TODO: pull value from configuration

    private TcpClient elemem;
    private HostPCListener listener;
    private int heartbeatCount = 0;

    public ElememInterface(InterfaceManager _im) {
        im = _im;
        listener = new HostPCListener(this);
        Start();
        Do(new EventBase(Connect));
    }

    ~ElememInterface() {
        Disconnect();
    }

    public NetworkStream GetWriteStream() {
        // only one writer can be active at a time
        if(elemem == null) {
            throw new InvalidOperationException("Socket not initialized.");
        }

        return elemem.GetStream();
    }

    public NetworkStream GetReadStream() {
        // only one reader can be active at a time
        if(elemem == null) {
            throw new InvalidOperationException("Socket not initialized.");
        }

        return elemem.GetStream();
    }

    public override void Connect() {
        elemem = new TcpClient(); 

        try {
            IAsyncResult result = elemem.BeginConnect((string)im.GetSetting("ip"), (int)im.GetSetting("port"), null, null);
            result.AsyncWaitHandle.WaitOne(messageTimeout);
            elemem.EndConnect(result);
        }
        catch(SocketException) {
            im.Do(new EventBase<string>(im.SetHostPCStatus, "ERROR")); 
            throw new OperationCanceledException("Failed to Connect");
        }

        im.Do(new EventBase<string>(im.SetHostPCStatus, "INITIALIZING")); 

        _ = listener.Listen(GetReadStream());
        SendAndWait("CONNECTED", new Dictionary<string, object>(), 
                    "CONNECTED_OK", messageTimeout);

        Dictionary<string, object> configDict = new Dictionary<string, object>();
        configDict.Add("stim_mode", (string)im.GetSetting("stimMode"));
        configDict.Add("experiment", (string)im.GetSetting("experimentName"));
        configDict.Add("subject", (string)im.GetSetting("participantCode"));
        configDict.Add("session", (int)im.GetSetting("session"));
        SendAndWait("CONFIGURE", configDict,
                    "CONFIGURE_OK", messageTimeout);

        // excepts if there's an issue with latency, else returns
        DoLatencyCheck();

        // start heartbeats
        int interval = (int)im.GetSetting("heartbeatInterval");
        DoRepeating(new EventBase(Heartbeat), -1, 0, interval);

        SendMessage("READY", new Dictionary<string, object>());
        im.Do(new EventBase<string>(im.SetHostPCStatus, "READY")); 
    }

    public override void Disconnect() {
        listener?.CancelRead();
        elemem.Close();
        elemem.Dispose();
    }

    private void DoLatencyCheck() {
        // except if latency is unacceptable
        Stopwatch sw = new Stopwatch();
        float[] delay = new float[20];

        // send 20 heartbeats, except if max latency is out of tolerance
        for(int i=0; i < 20; i++) {
            sw.Restart();
            Heartbeat();
            sw.Stop();

            // calculate manually to have sub millisecond resolution,
            // as ElapsedMilliseconds returns an integer number of
            // milliseconds.
            delay[i] = sw.ElapsedTicks * (1000f / Stopwatch.Frequency);

            if(delay[i] > 20) {
                throw new TimeoutException("Network Latency too large.");
            }

            // send new heartbeat every 50 ms
            Thread.Sleep(50 - (int)delay[i]);
        }
        
        float max = delay.Max();
        float mean = delay.Sum() / delay.Length;

        // the maximum resolution of the timer in nanoseconds
        long acc = (1000L*1000L*1000L) / Stopwatch.Frequency;

        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("max_latency", max);
        dict.Add("mean_latency", mean);
        dict.Add("accuracy", acc);

        im.Do(new EventBase<string, Dictionary<string, object>>(im.ReportEvent, "latency check", dict));
    }

    public override JObject SendAndWait(string type, Dictionary<string, object> data,
                                        string response, int timeout) {
        JObject json;
        var sw = new Stopwatch();
        var remainingWait = timeout;
        sw.Start();

        SendMessage(type, data);
        while(remainingWait > 0) {
            // inspect queue for incoming read messages, wait on
            // loop wait handle until timeout or message received.
            // This will block other reads/writes on the loop thread
            // until the message is received or times out. This doesn't
            // modify the queue, so operations are still ordered and
            // subsequent waits will complete successfully

            // wait on EventLoop wait handle, signalled when event added to queue
            wait.Reset();
            wait.Wait(remainingWait);

            remainingWait = remainingWait - (int)sw.ElapsedMilliseconds;

            // NOTE: this is inefficient due to looping over the full
            // collection on every wake, but this queue shouldn't ever
            // get beyond ~10 events.
            foreach(IEventBase ev in eventQueue) {
                // try to convert, null events are writes or other
                // actions
                var msgEv = ev as MessageEvent<NetMsg>;

                if(msgEv == null) {
                    continue;
                }

                json = JObject.Parse(msgEv.msg.msg);
                if(json.GetValue("type").Value<string>() == response) {
                    // once this chain returns, Loop continue on to 
                    // process all messages in queue
                    return json;
                }
            }
        }

        throw new TimeoutException();
    }

    public override void HandleMessage(NetMsg msg) {
        JObject json = JObject.Parse(msg.msg);
        string type = json.GetValue("type").Value<string>();
        ReportMessage(msg, false);

        if(type.Contains("ERROR")) {
            throw new Exception("Error received from Host PC.");
        }

        if(type == "EXIT") {
            // FIXME: call QUIT
            throw new Exception("Error received from Host PC.");
        }
    }

    public override void SendMessage(string type, Dictionary<string, object> data) {
        DataPoint point = new DataPoint(type, DataReporter.TimeStamp(), data);
        string message = point.ToJSON();

        Byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);

        NetworkStream stream = GetWriteStream();
        stream.Write(bytes, 0, bytes.Length);
        ReportMessage(new NetMsg(message, DataReporter.TimeStamp()), true);
    }

    private void Heartbeat()
    {
        var data = new Dictionary<string, object>();
        data.Add("count", heartbeatCount);
        heartbeatCount++;
        SendAndWait("HEARTBEAT", data, "HEARTBEAT_OK", heartbeatTimeout);
    }

    private void ReportMessage(NetMsg msg, bool sent)
    {
        Dictionary<string, object> messageDataDict = new Dictionary<string, object>();
        messageDataDict.Add("message", msg.msg);
        messageDataDict.Add("sent", sent);

        im.Do(new EventBase<string, Dictionary<string, object>, DateTime>(im.ReportEvent, "network", 
                                messageDataDict, msg.time));
    }
}