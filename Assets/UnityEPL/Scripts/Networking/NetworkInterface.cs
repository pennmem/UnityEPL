//#define NETWORKINTERFACE_DEBUG_MESSAGES

using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEPL {
    public abstract class NetworkInterface : EventLoop {
        private TcpClient tcpClient;
        private NetworkStream stream;

        private bool stopListening = false;
        private readonly List<(string, TaskCompletionSource<JObject>)> receiveRequests = new();

        private readonly static int connectionTimeoutMs = 5000;
        private readonly static int sendTimeoutMs = 5000;
        private readonly static int receiveTimeoutMs = 5000;

        public NetworkInterface() {
            Connect();
        }
        ~NetworkInterface() {
            // TODO: JPB: (needed) NetworkInterface::stopListening variable may not be needed
            DisconnectHelper();
        }

        protected void Connect() {
            DoWaitFor(ConnectHelper).Wait();
        }
        private async Task ConnectHelper() {
            tcpClient = new TcpClient();
            tcpClient.SendTimeout = sendTimeoutMs;

            // TODO: JPB: (needed) (bug) NetworkInterface always uses Elemem IP
            Task connectTask = tcpClient.ConnectAsync(Config.elememServerIP, Config.elememServerPort);
            var timeoutMessage = $"{this.GetType().Name} connection attempt timed out after {connectionTimeoutMs}ms";
            await TimeoutTask(connectTask, connectionTimeoutMs, timeoutMessage);
            if (connectTask.IsFaulted) {
                throw new OperationCanceledException($"{this.GetType().Name} connection attempt failed", connectTask.Exception);
            }
            stream = tcpClient.GetStream();

            DoListenerForever();
        }

        protected void Disconnect() {
            Do(DisconnectHelper);
        }
        private void DisconnectHelper() {
            stopListening = true;
            stream.Close();
            tcpClient.Close();
        }

        private void DoListenerForever() {
            Do(ListenerHelper);
        }
        private async Task ListenerHelper() {
            var buffer = new byte[8192];
            string messageBuffer = "";
            while (!stopListening && !cts.Token.IsCancellationRequested) {
                var bytesRead = await stream.ReadAsync(buffer, cts.Token);
                messageBuffer += Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Extract a full message from the byte buffer, and leave remaining characters in string buffer.
                // Also, if there is more than one message in the buffer, report both.
                var newLineIndex = 0;
                while (newLineIndex != messageBuffer.Length) {
                    newLineIndex = messageBuffer.IndexOf("\n", newLineIndex) + 1;
                    string message = messageBuffer.Substring(0, newLineIndex);

                    JObject json;
                    try {
                        // Check if value is a valid json
                        json = JObject.Parse(message);
                        // Remove it from the message buffer if it is valid
                        messageBuffer = messageBuffer.Substring(newLineIndex);
                        newLineIndex = 0;
                    } catch {
                        continue;
                    }
                    
                    // Report the message and send it to the waiting tasks
                    var msgType = json.GetValue("type").Value<string>();
                    ReportNetworkMessage(msgType, message, DataReporter.TimeStamp(), false);
                    for (int i = receiveRequests.Count - 1; i >= 0; i--) {
                        var (type, tcs) = receiveRequests[i];
                        if (type == msgType) {
                            receiveRequests.RemoveAt(i);
                            tcs.SetResult(json);
                        }
                    }

                    // Handle network error messages
                    if (msgType.Contains("ERROR")) {
                        throw new Exception($"Error received from {this.GetType().Name}");
                    }

                    // Handle network exit messge
                    if (msgType == "EXIT") {
                        Disconnect();
                    }
                }
            }
        }

        protected virtual Task<JObject> Receive(string type) {
            return ReceiveJson(type);
        }
        protected Task<JObject> ReceiveJson(string type) {
            TaskCompletionSource<JObject> tcs = new();
            receiveRequests.Add((type, tcs));
            var timeoutMessage = $"{this.GetType().Name} didn't receive message after waiting {receiveTimeoutMs}ms";
            return TimeoutTask(tcs.Task, receiveTimeoutMs, timeoutMessage);
        }

        protected virtual Task Send(string type, Dictionary<string, object> data = null) {
            return SendJson(type, data);
        }
        protected Task SendJson(string type, Dictionary<string, object> data = null) {
            DataPoint point = new DataPoint(type, data);
            string message = point.ToJSON();

            ReportNetworkMessage(type, message, point.time, true);

            Byte[] buffer = Encoding.UTF8.GetBytes(message + "\n");
            var timeoutMessage = $"{this.GetType().Name} didn't receive message after waiting {1000}ms";
            var sendTask = stream.WriteAsync(buffer, 0, buffer.Length);
            return TimeoutTask(sendTask, 1000, timeoutMessage);
        }

        protected Task<JObject> SendAndReceive(string sendType, string receiveType) {
            return SendAndReceive(sendType, null, receiveType);
        }
        protected Task<JObject> SendAndReceive(string sendType, Dictionary<string, object> sendData, string receiveType) {
            var task = Receive(receiveType);
            _ = Send(sendType, sendData);
            return task;
        }

        protected void ReportNetworkMessage(string type, string message, DateTime time, bool sent) {
            Dictionary<string, object> dict = new() {
                { "message", message },
                { "ip", ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString() },
                { "sent", sent },
            };

            var sendStr = sent ? "Sending" : "Received";
#if NETWORKINTERFACE_DEBUG_MESSAGES
            UnityEngine.Debug.Log($"{this.GetType().Name} {sendStr} Network Message: {type}\n{string.Join(Environment.NewLine, message)}");
#endif // NETWORKINTERFACE_DEBUG_MESSAGES

            manager.eventReporter.ReportScriptedEvent("network", time, dict);
        }
    }

}