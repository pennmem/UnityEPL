using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class YieldedEvent : IEnumerator {
    protected Stack<IEnumerator> _enumerators;
    protected readonly object _lock = new object();
    protected bool _completed = false;
    protected Object current = null;

    // TODO: JPB: Pass in a wait signal to indicate when it is done
    //            This would be used in the EventLoop
    public YieldedEvent(IEnumerator enumerator) {
        _enumerators = new Stack<IEnumerator>();
        _enumerators.Push(enumerator);
    }

    public object Current {
        get {
            lock (_lock) {
                return current;
                return _enumerators.Peek().Current;
            }
        }
    }

    // This function should only be called from one thread at a time
    // The lock is there to prevent this
    public virtual bool MoveNext() {
        lock (_lock) {
            while (_enumerators.Count > 0) {
                IEnumerator currentEnumerator = _enumerators.Peek();
                if (currentEnumerator.MoveNext()) {
                    var current = currentEnumerator.Current;
                    if (current is IEnumerator) {
                        _enumerators.Push((IEnumerator)current);
                    } else {
                        this.current = current;
                        return true;
                    }
                } else {
                    _enumerators.Pop();
                }
            }
            _completed = true;
            return false;
        }
    }

    public bool IsCompleted() {
        lock (_lock) {
            return _completed;
        }
    }

    public void Reset() {
        throw new NotSupportedException();
    }
}

public class YieldedEventQueue {
    protected ConcurrentQueue<Action> eventQueue = new ConcurrentQueue<Action>();
    protected List<YieldedEvent> yieldedEvents = new List<YieldedEvent>();
    protected List<RepeatingEvent> repeatingEvents = new List<RepeatingEvent>();

    protected volatile int threadID = -1;
    protected volatile bool running = true;

    public YieldedEventQueue() {
        this.threadID = Thread.CurrentThread.ManagedThreadId;
    }

    // TODO: JPB: Add Do that takes an Action or IEvent?
    public virtual void Do(IEnumerator thisEvent) {
        var yieldedEvent = new YieldedEvent(thisEvent);
        eventQueue.Enqueue(() => {
            if (yieldedEvent.MoveNext()) {
                // Add event to yieldedEvents list if it didn't finish
                yieldedEvents.Add(yieldedEvent);
            }
        });
    }

    // TODO: JPB: Add DoIn
    public virtual void DoIn(IEnumerator thisEvent, int delay) {
        throw new NotImplementedException();
    }

    // TODO: JPB: Add DoRepeating
    public virtual void DoRepeating(IEnumerator thisEvent, int iterations, int delay, int interval) {
        throw new NotImplementedException();
    }
    public virtual void DoRepeating(RepeatingEvent thisEvent) {
        throw new NotImplementedException();
    }

    // Avoid using this if possible
    public virtual IEnumerator DoBlocking(IEnumerator thisEvent) {
        var yieldedEvent = new YieldedEvent(thisEvent);
        eventQueue.Enqueue(() => {
            if (yieldedEvent.MoveNext()) {
                // Add event to yieldedEvents list if it didn't finish
                yieldedEvents.Add(yieldedEvent);
            }
        });

        while (!yieldedEvent.IsCompleted()) {
            yield return null;
        }
        yield break;
    }

    // Avoid using this if possible
    public virtual IEnumerator<T> DoGet<T>(IEnumerator thisEvent) {
        var yieldedEvent = new YieldedEvent(thisEvent);
        eventQueue.Enqueue(() => {
            if (yieldedEvent.MoveNext()) {
                // Add event to yieldedEvents list if it didn't finish
                yieldedEvents.Add(yieldedEvent);
            }
        });

        var waiting = default(T);
        while (!yieldedEvent.IsCompleted()) {
            yield return waiting;
        }

        yield return (T)yieldedEvent.Current;

    }

    public bool Process() {
        // Evaluate all yielded events
        foreach (var yieldedEvent in yieldedEvents) {
            yieldedEvent.MoveNext();
        }
        yieldedEvents.RemoveAll(x => x.IsCompleted());

        // Run the new event
        Action thisEvent;
        if (running && eventQueue.TryDequeue(out thisEvent)) {
            try {
                thisEvent.Invoke();
            } catch (Exception e) {
                ErrorNotification.Notify(e);
            }
            return true;
        }
        return false;
    }

    public bool IsRunning() {
        return running;
    }

    public void Pause(bool pause) {
        running = !pause;

        foreach (RepeatingEvent re in repeatingEvents) {
            re.Pause(pause);
        }
    }

    public void ClearRepeatingEvent(RepeatingEvent thisEvent) {
        if (repeatingEvents.Contains(thisEvent)) {
            repeatingEvents.Remove(thisEvent);
        }
    }
}

public class RepeatingEvent {
    internal void Pause(bool pause) {
        //throw new NotImplementedException();
    }

    internal void Stop() {
        //throw new NotImplementedException();
    }
}

internal class ErrorNotification {
    internal static void Notify(Exception e) {
        throw new NotImplementedException();
    }
}


