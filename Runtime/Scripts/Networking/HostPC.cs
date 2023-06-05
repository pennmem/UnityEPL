using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnityEPL {

    public abstract class HostPC : NetworkInterface {
        public enum StateMsg {
            // No extra data
            REST, ORIENT, COUNTDOWN, TRIALEND, DISTRACT, INSTRUCT, WAITING, SYNC, VOCALIZATION,
            // No extra data, ramulator copy over (likely unused)
            ENCODING, RETRIEVAL,
            // extra data: (float duration)
            ISI, RECALL,
        }
        public enum CLMsg {
            STIM,
            SHAM,
            NORMALIZE,
        }
        public enum CCLMsg {
            PAUSE_STIM,
            RESUME_STIM,
            STOP_STIM,
        }

        protected abstract Task DoLatencyCheck();
        protected abstract CancellationTokenSource DoHeartbeatsForever();

        public abstract Task Configure();
        public abstract Task Quit();

        public abstract Task SendMathMsg(string problem, string response, int responseTimeMs, bool correct);
        public abstract Task SendStimSelectMsg(string tag);
        public abstract Task SendStimMsg();
        public abstract Task SendCLMsg(CLMsg type, uint classifyMs);
        public abstract Task SendCCLStartMsg(int durationS);
        public abstract Task SendCCLMsg(CCLMsg type);
        public abstract Task SendSessionMsg(int session);
        public abstract Task SendStateMsg(StateMsg state, Dictionary<string, object> extraData = null);
        public abstract Task SendTrialMsg(int trial, bool stim);
        public abstract Task SendWordMsg(string word, int serialPos, bool stim, Dictionary<string, object> extraData = null);
        public abstract Task SendExitMsg();
        public abstract Task SendLogMsg(string type, Dictionary<string, object> data = null);
    }

}