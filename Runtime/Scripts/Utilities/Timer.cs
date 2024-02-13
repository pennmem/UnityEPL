using System;

namespace UnityEPL {
    public struct Timer {
        DateTime startTime;
        DateTime stopTime;

        public Timer(TimeSpan duration) {
            this.startTime = Clock.UtcNow;
            this.stopTime = startTime + duration;
        }

        public Timer(DateTime stopTime) {
            this.startTime = Clock.UtcNow;
            this.stopTime = stopTime;
        }

        public bool IsFinished() {
            return Clock.UtcNow >= stopTime;
        }
    }
}