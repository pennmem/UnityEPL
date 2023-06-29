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
            REST, ORIENT, COUNTDOWN, TRIAL, TRIALEND, DISTRACT, INSTRUCT, WAITING, SYNC, VOCALIZATION,
            // No extra data, ramulator copy over (likely unused)
            ENCODING, RETRIEVAL, WORD, MATH,
            // extra data: (float duration)
            ISI, RECALL, FINAL_RECALL,
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

        protected abstract Task DoLatencyCheckTS();
        protected abstract CancellationTokenSource DoHeartbeatsForeverTS();

        public abstract Task ConfigureTS();
        public abstract Task QuitTS();

        public abstract Task SendMathMsgTS(string problem, string response, int responseTimeMs, bool correct);
        public abstract Task SendStimSelectMsgTS(string tag);
        public abstract Task SendStimMsgTS();
        public abstract Task SendCLMsgTS(CLMsg type, uint classifyMs);
        public abstract Task SendCCLStartMsgTS(int durationS);
        public abstract Task SendCCLMsgTS(CCLMsg type);
        public abstract Task SendSessionMsgTS(int session);
        public abstract Task SendStateMsgTS(StateMsg state, Dictionary<string, object> extraData = null);
        public abstract Task SendTrialMsgTS(int trial, bool stim);
        public abstract Task SendWordMsgTS(string word, int serialPos, bool stim, Dictionary<string, object> extraData = null);
        public abstract Task SendExitMsgTS();
        public abstract Task SendLogMsgTS(string type, Dictionary<string, object> data = null);
    }

}