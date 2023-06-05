using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace UnityEPL {

    public abstract class DataHandler : EventMonoBehaviour {
        protected List<DataReporter> reportersToHandle = new();
        protected Queue<DataReporter> toAdd = new();
        protected Queue<DataReporter> toRemove = new();
        protected Queue<DataPoint> eventQueue = new();

        // TODO: JPB: (needed) (bug) DataHandler Update is overriden by child classes
        protected virtual void Update() {
            DataReporter result;

            if (toAdd.TryDequeue(out result)) {
                reportersToHandle.Add(result);
            }

            foreach (DataReporter reporter in reportersToHandle) {
                if (reporter.UnreadDataPointCountMB() > 0) {
                    DataPoint[] newPoints = reporter.ReadDataPointsMB(reporter.UnreadDataPointCountMB());
                    HandleDataPoints(newPoints);
                }
            }

            if (toRemove.TryDequeue(out result)) {
                if (!reportersToHandle.Remove(result)) {
                    toRemove.Enqueue(result);
                }
            }
        }

        // TODO: JPB: (needed) (bug) Make QueuePoint use a blittable type instead of DataPoint
        //            Or at least have it use Mutex
        public void QueuePoint(DataPoint data) {
            Do(() => { QueuePointHelper(data); });
        }
        public void QueuePointMB(DataPoint data) {
            DoMB(QueuePointHelper, data);
        }
        protected void QueuePointHelper(DataPoint data) {
            eventQueue.Enqueue(data);
        }

        public void AddReporterMB(DataReporter add) {
            DoMB(AddReporterHelper, add);
        }
        public void AddReporterHelper(DataReporter add) {
            toAdd.Enqueue(add);
        }

        public void RemoveReporterMB(DataReporter remove) {
            DoMB(RemoveReporterHelper, remove);
        }
        public void RemoveReporterHelper(DataReporter remove) {
            toRemove.Enqueue(remove);
        }

        protected abstract void HandleDataPoints(DataPoint[] dataPoints);
    }

}