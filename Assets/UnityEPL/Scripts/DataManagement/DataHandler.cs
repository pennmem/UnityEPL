using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public abstract class DataHandler : MonoBehaviour
{
    protected List<DataReporter> reportersToHandle = new List<DataReporter>();
    protected ConcurrentQueue<DataReporter> toAdd = new ConcurrentQueue<DataReporter>();
    protected ConcurrentQueue<DataReporter> toRemove = new ConcurrentQueue<DataReporter>();
    protected System.Collections.Concurrent.ConcurrentQueue<DataPoint> eventQueue = new ConcurrentQueue<DataPoint>();

    protected InterfaceManager im;

    public void Start() {
        im = FindObjectOfType<InterfaceManager>() as InterfaceManager;
    }

    public void QueuePoint(DataPoint data) {
        eventQueue.Enqueue(data);
    }

    protected virtual void Update()
    {
        DataReporter result;

        if(toAdd.TryDequeue(out result)) {
            reportersToHandle.Add(result);
        }

        foreach (DataReporter reporter in reportersToHandle)
        {
            if (reporter.UnreadDataPointCount() > 0)
            {
                DataPoint[] newPoints = reporter.ReadDataPoints(reporter.UnreadDataPointCount());
                HandleDataPoints(newPoints);
            }
        }

        if(toRemove.TryDequeue(out result)) {
            if(!reportersToHandle.Remove(result)) {
                toRemove.Enqueue(result);
            }
        }
    }

    public void AddReporter(DataReporter add) {
        toAdd.Enqueue(add);
    }

    public void RemoveReporter(DataReporter remove) {
        toRemove.Enqueue(remove);
    }

    protected abstract void HandleDataPoints(DataPoint[] dataPoints);
}