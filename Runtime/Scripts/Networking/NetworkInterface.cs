//#define NETWORKINTERFACE_DEBUG_MESSAGES

using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace UnityEPL {
    public abstract class NetworkInterface : EventLoop {
        private TcpClient tcpClient;
        private NetworkStream stream;

        // TODO: JPB: (needed) NetworkInterface::stopListening variable may not be needed
        private bool stopListening = false;
        private readonly List<(string, TaskCompletionSource<JObject>)> receiveRequests = new();

        private readonly static int connectionTimeoutMs = 5000;
        private readonly static int sendTimeoutMs = 5000;
        private readonly static int receiveTimeoutMs = 5000;

        ~NetworkInterface() {
            DisconnectHelper();
        }

        public async Task<bool> IsConnected() {
            return await DoGet<Bool>(IsConnectedHelper);
        }
        public Bool IsConnectedHelper() {
            return tcpClient.Connected;
        }

        public Task Connect() {
            return DoWaitFor(ConnectHelper);
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
            if (tcpClient?.Connected ?? false) {
                stopListening = true;
                stream.Close();
                tcpClient.Close();
            }
        }

        protected virtual void DoListenerForever() {
            Do(ListenerHelperJson);
        }
        private async Task ListenerHelperJson() {
            var buffer = new byte[8192];
            string messageBuffer = "";
            while (!stopListening && !cts.Token.IsCancellationRequested) {
                try {
                    var bytesRead = await stream.ReadAsync(buffer, cts.Token);
                    messageBuffer += Encoding.UTF8.GetString(buffer, 0, bytesRead);
                } catch (SocketException) {
                    var name = this.GetType().Name;
                    throw new IOException($"The network interface {name} disconnected prematurely");
                }

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
                    ReportNetworkMessage(msgType, message, Clock.UtcNow, false);
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
            return DoGetRelaxed(ReceiveJsonHelper, type.ToNativeText());
        }
        private Task<JObject> ReceiveJsonHelper(NativeText type) {
            TaskCompletionSource<JObject> tcs = new();
            receiveRequests.Add((type.ToString(), tcs));
            var timeoutMessage = $"{this.GetType().Name} didn't receive message after waiting {receiveTimeoutMs}ms";
            type.Dispose();
            return TimeoutTask(tcs.Task, receiveTimeoutMs, timeoutMessage);
        }

        // TODO: JPB: (needed) Make NetworkInterface::Send use Mutex
        protected virtual Task Send(string type, Dictionary<string, object> data = null) {
            return SendJson(type, data);
        }
        protected Task SendJson(string type, Dictionary<string, object> data = null) {
            return DoWaitFor(() => { SendJsonHelper(type, data); });
        }
        private Task SendJsonHelper(string type, Dictionary<string, object> data = null) {
            DataPoint point = new DataPoint(type, data);
            string message = point.ToJSON();

            Byte[] buffer = Encoding.UTF8.GetBytes(message + "\n");
            var timeoutMessage = $"{this.GetType().Name} didn't receive message after waiting {1000}ms";
            Task sendTask;
            try {
                sendTask = stream.WriteAsync(buffer, 0, buffer.Length);
            } catch (SocketException) {
                var name = this.GetType().Name;
                throw new IOException($"The network interface {name} closed before the {type} message could be sent");
            }

            ReportNetworkMessage(type, message, point.time, true);
            return TimeoutTask(sendTask, 1000, timeoutMessage);
        }

        protected Task<JObject> SendAndReceive(string sendType, string receiveType) {
            return SendAndReceive(sendType, null, receiveType);
        }
        protected async Task<JObject> SendAndReceive(string sendType, Dictionary<string, object> sendData, string receiveType) {
            var recvTask = Receive(receiveType);
            await Send(sendType, sendData);
            return await recvTask;
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

            manager.eventReporter.LogTS("network", time, dict);
        }
    }

}