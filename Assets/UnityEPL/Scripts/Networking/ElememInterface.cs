using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnityEPL {

    public interface HostPC {
        public void SetState();
        public void SendCCLStartMessage();
    }

    public class ElememInterface : NetworkInterface {
        public ElememInterface() { }

        public Task Configure() {
            return DoWaitFor(ConfigureHelper);
        }
        protected async Task ConfigureHelper() {
            // Configure Elemem
            await SendAndReceive("CONNECTED", "CONNECTED_OK");

            Dictionary<string, object> configDict = new() {
                { "stim_mode", Config.stimMode },
                { "experiment", Config.experimentName },
                { "subject", Config.subject },
                { "session", Config.session },
            };
            await SendAndReceive("CONFIGURE", configDict, "CONFIGURE_OK");

            // Latency Check
            await DoLatencyCheck();

            // Start Heartbeats
            DoHeartbeatsForever();

            // Start Elemem
            await Send("READY");
        }

        private uint heartbeatCount = 0;
        protected CancellationTokenSource DoHeartbeatsForever() {
            return DoRepeating(0, Config.elememHeartbeatInterval, null, DoHeartbeatHelper);
        }
        protected async Task DoHeartbeatHelper() {
            Dictionary<string, object> data = new() {
                { "count", heartbeatCount }
            };
            heartbeatCount++;
            await Send("HEARTBEAT", data);
            await Receive("HEARTBEAT_OK");
        }

        protected readonly static double maxSingleTimeMs = 20;
        protected readonly static double meanSingleTimeMs = 5;
        protected Task DoLatencyCheck() {
            return DoWaitFor(DoLatencyCheckHelper);
        }
        protected async Task DoLatencyCheckHelper() {
            DateTime startTime;
            double[] delay = new double[20];

            // Send 20 heartbeats, every 50ms, except if max latency is out of tolerance
            for (int i = 0; i < 20; i++) {
                UnityEngine.Debug.Log($"Latency Check {i}");
                startTime = Clock.UtcNow;
                await DoHeartbeatHelper();
                delay[i] = (Clock.UtcNow - startTime).TotalMilliseconds;

                if (delay[i] >= maxSingleTimeMs) {
                    throw new TimeoutException($"Single heartbeat time greater than {maxSingleTimeMs}ms");
                }

                await InterfaceManager.Delay(50 - (int)delay[i]);
            }

            // Check average latency
            double max = delay.Max();
            double mean = delay.Average();
            if (mean >= meanSingleTimeMs) {
                throw new TimeoutException($"Mean heartbeat time greater than {meanSingleTimeMs}ms");
            }

            // the maximum resolution of the timer in nanoseconds
            long acc = (1000L * 1000L * 1000L) / Stopwatch.Frequency;

            Dictionary<string, object> dict = new() {
                { "max_latency_ms", max },
                { "mean_latency_ms", mean },
                { "resolution_ns", acc },
            };
            manager.eventReporter.ReportScriptedEvent("latency check", dict);
            UnityEngine.Debug.Log(string.Join(Environment.NewLine, dict));
        }

        protected override async Task<JObject> Receive(string type) {
            var json = await ReceiveJson(type);
            var msgType = json.GetValue("type").Value<string>();
            if (msgType == "EXIT") {
                Disconnect();
                throw new InvalidOperationException("Elemem exited and ended it's connection");
            }
            return json;
        }
    }

}