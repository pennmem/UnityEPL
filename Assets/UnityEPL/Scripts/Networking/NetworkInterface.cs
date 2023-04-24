//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading;
//using System.Threading.Tasks;
//using Newtonsoft.Json.Linq;

//public struct NetMsg {
//    public string msg;
//    public DateTime time;

//    public NetMsg(string msg, DateTime time) {
//        this.msg = msg;
//        this.time = time;
//    }
//}

//public abstract class NetworkInterface : EventLoop {
//    public abstract void SendAndWait(string type, Dictionary<string, object> data,
//                                        string response, int timeout);
//    public abstract void Connect();
//    public abstract void Disconnect();
//    public abstract void HandleMessage(NetMsg msg);
//    public abstract void SendMessage(string type, Dictionary<string, object> data);

//    protected InterfaceManager manager;
//    protected TcpClient tcpClient;
//    protected NetworkListener listener;

//    protected int messageTimeout = 1000;
//    protected int heartbeatTimeout = 8000;
//    protected int heartbeatCount = 0;

//    protected NetworkInterface(InterfaceManager manager) {
//        this.manager = manager;
//        this.listener = new NetworkListener(this);
//    }

//    protected void Heartbeat(string prefix) {
//        var data = new Dictionary<string, object>();
//        data.Add("count", heartbeatCount);
//        heartbeatCount++;
//        SendAndWait(prefix + "HEARTBEAT", data, prefix + "HEARTBEAT_OK", heartbeatTimeout);
//    }

//    protected void DoLatencyCheck(string prefix = "") {
//        // except if latency is unacceptable
//        Stopwatch sw = new Stopwatch();
//        float[] delay = new float[20];

//        // send 20 heartbeats, except if max latency is out of tolerance
//        for (int i = 0; i < 20; i++) {
//            sw.Restart();
//            Heartbeat(prefix);
//            sw.Stop();

//            // calculate manually to have sub millisecond resolution,
//            // as ElapsedMilliseconds returns an integer number of
//            // milliseconds.
//            delay[i] = sw.ElapsedTicks * (1000f / Stopwatch.Frequency);

//            if (delay[i] > 20) {
//                throw new TimeoutException("Network Latency too large.");
//            }

//            // send new heartbeat every 50 ms
//            Thread.Sleep(50 - (int)delay[i]);
//        }

//        float max = delay.Max();
//        float mean = delay.Average();

//        // the maximum resolution of the timer in nanoseconds
//        long acc = (1000L * 1000L * 1000L) / Stopwatch.Frequency;

//        Dictionary<string, object> dict = new Dictionary<string, object>();
//        dict.Add("max_latency_ms", max);
//        dict.Add("mean_latency_ms", mean);
//        dict.Add("resolution_ns", acc);

//        SendMessage("latency check", dict);
//        manager.ReportEvent("latency check", dict);
//        UnityEngine.Debug.Log(string.Join(Environment.NewLine, dict));
//    }

//    protected void ReportNetworkMessage(NetMsg msg, bool sent) {
//        Dictionary<string, object> messageDataDict = new Dictionary<string, object>();
//        messageDataDict.Add("message", msg.msg);
//        messageDataDict.Add("ip", ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString());
//        messageDataDict.Add("sent", sent);

//        manager.ReportEvent("network", msg.time, messageDataDict);
//    }

//    protected NetworkStream GetWriteStream() {
//        // only one writer can be active at a time
//        if (tcpClient == null) {
//            throw new InvalidOperationException("Socket not initialized.");
//        }

//        return tcpClient.GetStream();
//    }

//    protected NetworkStream GetReadStream() {
//        // only one reader can be active at a time
//        if (tcpClient == null) {
//            throw new InvalidOperationException("Socket not initialized.");
//        }

//        return tcpClient.GetStream();
//    }
//}

//public class NetworkListener : MessageHandler<NetMsg> {

//    // FIXME: catch socket exceptions to give human readable errors

//    Byte[] buffer;
//    const Int32 bufferSize = 2048;

//    CancellationTokenSource tokenSource;

//    string messageBuffer = "";
//    public NetworkListener(NetworkInterface networkInterface)
//            : base(networkInterface, networkInterface.HandleMessage) {
//        buffer = new Byte[bufferSize];
//    }

//    public bool IsListening() {
//        return tokenSource?.IsCancellationRequested ?? false;
//    }

//    public async Task<NetMsg> Listen(NetworkStream stream) {
//        /*
//        Asynchronously listens for a single network message and
//        passes it to the host message queue through the action
//        used to construct this listener.
//        */

//        if (IsListening()) {
//            throw new AccessViolationException("Already Listening");
//        }

//        if (tokenSource == null) {
//            tokenSource = new CancellationTokenSource();
//        }

//        int bytesRead;
//        do {
//            bytesRead = await stream.ReadAsync(buffer, 0, bufferSize, tokenSource.Token);
//        } while (!ParseBuffer(bytesRead, out NetMsg message));

//        Do(message);

//        if (!tokenSource.IsCancellationRequested) {
//            _ = Listen(stream);
//        } else {
//            tokenSource.Dispose();
//            tokenSource = null;
//        }

//        return message;
//    }

//    public void CancelRead() {
//        tokenSource?.Cancel();
//    }

//    private bool ParseBuffer(int bytesRead, out NetMsg msgResult) {
//        /*
//        Extract a full message from the byte buffer, and leave remaining characters
//        in string buffer. Return true if message terminating newline is read from string, 
//        false otherwise.
//        */

//        messageBuffer += System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

//        string message = messageBuffer.Substring(0, messageBuffer.IndexOf("\n") + 1);
//        messageBuffer = messageBuffer.Substring(messageBuffer.IndexOf("\n") + 1);

//        msgResult = new NetMsg(message, DataReporter.TimeStamp());

//        return message.Length > 0;
//    }
//}
