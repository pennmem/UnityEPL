using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnityEPL {

    public class ElememInterface : HostPC {
        public ElememInterface() { }

        public override Task ConfigureTS() {
            return DoWaitForTS(ConfigureHelper);
        }
        protected async Task ConfigureHelper() {
            // Configure Elemem
            await SendAndReceiveTS("CONNECTED", "CONNECTED_OK");

            Dictionary<string, object> configDict = new() {
                { "stim_mode", Config.stimMode },
                { "experiment", Config.experimentName },
                { "subject", Config.subject },
                { "session", Config.sessionNum },
            };
            await SendAndReceive("CONFIGURE", configDict, "CONFIGURE_OK");

            // Latency Check
            await DoLatencyCheckTS();

            // Start Heartbeats
            DoHeartbeatsForeverTS();

            // Start Elemem
            await SendTS("READY");
        }

        public override async Task QuitTS() {
            await SendExitMsgTS();
            DisconnectTS();
        }

        private uint heartbeatCount = 0;
        protected override CancellationTokenSource DoHeartbeatsForeverTS() {
            return DoRepeatingTS(0, Config.elememHeartbeatInterval, null, DoHeartbeatHelper);
        }
        protected async Task DoHeartbeatHelper() {
            Dictionary<string, object> data = new() {
                { "count", heartbeatCount }
            };
            heartbeatCount++;
            await SendAndReceive("HEARTBEAT", data, "HEARTBEAT_OK");
        }

        protected readonly static double maxSingleTimeMs = 20;
        protected readonly static double meanSingleTimeMs = 5;
        protected override Task DoLatencyCheckTS() {
            return DoWaitForTS(DoLatencyCheckHelper);
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
            manager.eventReporter.LogTS("latency check", dict);
            UnityEngine.Debug.Log(string.Join(Environment.NewLine, dict));
        }

        protected override async Task<JObject> ReceiveTS(string type) {
            var json = await ReceiveJsonTS(type);
            var msgType = json.GetValue("type").Value<string>();
            if (msgType == "EXIT") {
                DisconnectTS();
                throw new InvalidOperationException("Elemem exited and ended it's connection");
            }
            return json;
        }

        public override Task SendMathMsgTS(string problem, string response, int responseTimeMs, bool correct) {
            Dictionary<string, object> data = new() {
                { "problem", problem },
                { "response", response },
                { "response_time_ms", responseTimeMs },
                { "correct", correct },
            };
            return SendTS("MATH", data);
        }

        public override Task SendStimSelectMsgTS(string tag) {
            Dictionary<string, object> data = new() {
                { "stimtag", tag },
            };
            return SendTS("STIMSELECT", data);
        }

        public override Task SendStimMsgTS() {
            return SendTS("STIM");
        }

        public override Task SendCLMsgTS(HostPcClMsg type) {
            return SendTS(type.name, type.dict);
        }

        public override Task SendCCLMsgTS(HostPcCclMsg type) {
            return SendTS(type.name, type.dict);
        }

        public override Task SendSessionMsgTS(int session) {
            Dictionary<string, object> data = new() {
                { "session", session },
            };
            return SendTS("SESSION", data);
        }

        public override Task SendStateMsgTS(HostPcStateMsg state, Dictionary<string, object> extraData = null) {
            var dict = (extraData != null) ? new Dictionary<string, object>(extraData) : new();
            foreach (var item in state.dict) {
                dict.Add(item.Key, item.Value);
            }
            return SendTS(state.name, dict);
        }

        public override Task SendTrialMsgTS(int trial, bool stim) {
            Dictionary<string, object> data = new() {
                { "trial", trial },
                { "stim", stim },
            };
            return SendTS("TRIAL", data);
        }

        public override Task SendWordMsgTS(string word, int serialPos, bool stim, Dictionary<string, object> extraData = null) {
            var data = (extraData != null) ? new Dictionary<string, object>(extraData) : new();
            data["word"] = word;
            data["serialPos"] = serialPos;
            data["stim"] = stim;  
            return SendTS("WORD", data);
        }

        public override Task SendExitMsgTS() {
            return SendTS("EXIT");
        }

        public override Task SendLogMsgTS(string type, Dictionary<string, object> data = null) {
            return SendTS(type, data);
        }
    }

}