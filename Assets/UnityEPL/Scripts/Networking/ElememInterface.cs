using Codice.CM.Client.Differences.Graphic;
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

        public override Task Configure() {
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

        public override async Task Quit() {
            await SendExitMsg();
            Disconnect();
        }

        private uint heartbeatCount = 0;
        protected override CancellationTokenSource DoHeartbeatsForever() {
            return DoRepeating(0, Config.elememHeartbeatInterval, null, DoHeartbeatHelper);
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
        protected override Task DoLatencyCheck() {
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

        public override Task SendMathMsg(string problem, string response, int responseTimeMs, bool correct) {
            Dictionary<string, object> data = new() {
                { "problem", problem },
                { "response", response },
                { "response_time_ms", responseTimeMs },
                { "correct", correct },
            };
            return Send("MATH", data);
        }

        public override Task SendStimSelectMsg(string tag) {
            Dictionary<string, object> data = new() {
                { "stimtag", tag },
            };
            return Send("STIMSELECT", data);
        }

        public override Task SendStimMsg() {
            return Send("STIM");
        }

        public override Task SendCLMsg(CLMsg type, uint classifyMs) {
            Dictionary<string, object> data = new() {
                { "classifyms", classifyMs },
            };
            return Send(Enum.GetName(typeof(CLMsg), type), data);
        }

        public override Task SendCCLStartMsg(int durationS) {
            Dictionary <string, object> data = new() {
                { "duration", durationS}
            };
            return Send("CCLSTARTSTIM", data);
        }

        public override Task SendCCLMsg(CCLMsg type) {
            return Send(Enum.GetName(typeof(CCLMsg), type));
        }

        public override Task SendSessionMsg(int session) {
            Dictionary<string, object> data = new() {
                { "session", session },
            };
            return Send("SESSION", data);
        }

        public override Task SendStateMsg(StateMsg state, Dictionary<string, object> extraData = null) {
            return Send(Enum.GetName(typeof(StateMsg), state), extraData);
        }

        public override Task SendTrialMsg(int trial, bool stim) {
            Dictionary<string, object> data = new() {
                { "trial", trial },
                { "stim", stim },
            };
            return Send("TRIAL", data);
        }

        public override Task SendWordMsg(string word, int serialPos, bool stim, Dictionary<string, object> extraData = null) {
            var data = (extraData != null) ? new Dictionary<string, object>(extraData) : new();
            data["word"] = word;
            data["serialPos"] = serialPos;
            data["stim"] = stim;  
            return Send("WORD", data);
        }

        public override Task SendExitMsg() {
            return Send("EXIT");
        }

        public override Task SendLogMsg(string type, Dictionary<string, object> data = null) {
            return Send(type, data);
        }
    }

}