using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System;
using UnityEngine;

public class EventQueue {
    public ConcurrentQueue<IEventBase> eventQueue;

    protected int repeatingEventID = 0;
    protected List<RepeatingEvent> repeatingEvents = new List<RepeatingEvent>();
    protected volatile bool running = true;

    public virtual void Do(IEventBase thisEvent) {
        eventQueue.Enqueue(thisEvent);
    }

    public virtual void DoIn(IEventBase thisEvent, int delay) {
        if(Running()) {
            RepeatingEvent repeatingEvent = new RepeatingEvent(thisEvent, 1, delay, Timeout.Infinite, this);
            DoRepeating(repeatingEvent);
        }
        else {
            throw new Exception("Can't add timed event to non running Queue");
        }
    }

    public virtual void DoRepeating(RepeatingEvent thisEvent) {
        // enqueues repeating event at set intervals. If timer isn't
        // stopped, stopping processing thread will still stop execution
        // of events

        if(Running()) {
            // use Do to synchronize repeatingEvents access
            Do(new EventBase<RepeatingEvent>(repeatingEvents.Add, thisEvent));
        }
        else {
            throw new Exception("Can't enqueue repeating event to non running Queue");
        }
    }

    public virtual void DoRepeating(IEventBase thisEvent, int iterations, int delay, int interval) {
        // timers should only be created if running
        DoRepeating(new RepeatingEvent(thisEvent, iterations, delay, interval, this));
    }

    // Process one event in the queue.
    // Returns true if an event was available to process.
    public bool Process() {
        IEventBase thisEvent;
        if (running && eventQueue.TryDequeue(out thisEvent)) {
            try {
                thisEvent.Invoke();
            } catch(Exception e) {
                ErrorNotification.Notify(e);
            }
            return true;
        }
        return false;
    }

    public EventQueue() {
        eventQueue = new ConcurrentQueue<IEventBase>();
    }

    public bool Running() {
        return running;
    }

    public void Pause(bool pause) {
        running = !pause;
        
        foreach(RepeatingEvent re in repeatingEvents) {
            re.Pause(pause);
        }
    }

    public void ClearRepeatingEvent(RepeatingEvent thisEvent) {
        if(repeatingEvents.Contains(thisEvent)) {
            repeatingEvents.Remove(thisEvent);
        }
    }
}

public interface IEventBase {
    void Invoke();
}

public class EventBase : IEventBase {
    protected Action EventAction;

    public virtual void Invoke() {
        EventAction?.Invoke();
    }

    public EventBase(Action thisAction) {
        EventAction = thisAction;
    }
}

// Wrapper class to allow different delegate signatures
// in Event Manager
public class EventBase<T> : EventBase {
    public EventBase(Action<T> thisAction, T t) : base(() => thisAction(t)) {}
}

public class EventBase<T, U> : EventBase {
    public EventBase(Action<T, U> thisAction, T t, U u) : base(() => thisAction(t, u)) {}
}

public class EventBase<T, U, V> : EventBase {
    public EventBase(Action<T, U, V> thisAction, T t, U u, V v) : base(() => thisAction(t, u, v)) {}
}
public class EventBase<T, U, V, W> : EventBase {
    public EventBase(Action<T, U, V, W> thisAction, T t, U u, V v, W w) : base(() =>thisAction(t, u, v, w)) {}
}

// public class WaitEvent : IEventBase {
//     // expose Wait handle to wait for invoke to be called
//     private readonly ManualResetEventSlim wait = new ManualResetEventSlim();

//     public void Invoke() {
//         base.Invoke();
//         wait.Set();
//     }
// }

public class RepeatingEvent : IEventBase {

    private int iterations;
    private readonly int maxIterations;
    private int delay;
    private int interval;
    public readonly ManualResetEventSlim flag;
    private Timer timer;
    private DateTime startTime;
    private IEventBase thisEvent;
    private EventQueue queue;

    public RepeatingEvent(IEventBase originalEvent, int _iterations, int _delay, int _interval,
                          EventQueue _queue, ManualResetEventSlim _flag = null) {
        maxIterations = _iterations;
        delay = _delay;
        interval = _interval;
        startTime = HighResolutionDateTime.UtcNow;
        queue = _queue;


        if(_flag == null) {
            flag = new ManualResetEventSlim();
        }
        else {
            flag = _flag;
        }

        thisEvent = originalEvent;
        SetTimer();
    }

    public RepeatingEvent(Action _action, int _iterations, int _delay,
                          int _interval, EventQueue _queue,
                          ManualResetEventSlim _flag = null) 
                          : this(new EventBase(_action), 
                                 _iterations, _delay, 
                                 _interval, _queue, _flag) {}

    private void SetTimer() {
        this.timer = new Timer(delegate(System.Object obj) { 
                                                            // event is a keyword
                                                            var evnt = (RepeatingEvent)obj;
                                                            if(!evnt.flag.IsSet) {queue.Do(evnt);} 
                                                           }, 
                                                this, delay, interval);
    }

    public void Invoke() {
        if(!(maxIterations < 0) && (iterations >= maxIterations)) {
            Stop();
            return;
        }

        Interlocked.Increment(ref this.iterations);
        thisEvent.Invoke();
    }

    public void Pause(bool pause) {
        DateTime time = HighResolutionDateTime.UtcNow;
        // examples don't check success of Change
        if(pause) {
            flag.Set();
            delay -= (int)((TimeSpan)(time - startTime)).TotalMilliseconds;
            if(delay <=0) {
                // Set delay to be the remaining portion of interval
                // |----||----------||-----------|
                // delay    interval    interval
                //         elapsed          |
                // remaining = interval - (elapsed - delay) % interval
                // C# mod is remainder, rather than mod,
                // so it's happy with negative values here
                delay = interval + (delay % interval);
            }
            timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }
        else {
            flag.Reset();
            startTime = time;
            timer?.Change(delay, interval);
        }
    }

    public void Stop() {
        flag.Set();
        timer?.Dispose();
        // set timer to null in case Pause is queued before event is removed
        timer = null;
        queue.Do(new EventBase<RepeatingEvent>(queue.ClearRepeatingEvent, this));
    }
}