using System;
using System.Threading;
using System.Collections.Concurrent;

public class EventLoop : EventQueue {
    protected ManualResetEventSlim wait;
    private CancellationTokenSource tokenSource;

    public EventLoop()
    {
        wait = new ManualResetEventSlim();
        running = false;
    }

    ~EventLoop()
    {
        wait.Dispose();
    }

    public void Start(){
        if(Running()) {
            return;
        }

        running = true;
        Thread loop = new Thread(Loop);

        tokenSource = new CancellationTokenSource();
        loop.Start(tokenSource.Token);
    }

    public void Stop(){
        if(!Running()) {
            return;
        }

        running = false;
        tokenSource.Cancel();
        tokenSource.Dispose();
        wait.Set();
        StopTimers();
    }

    public void StopTimers() {
        foreach(var re in repeatingEvents) {
            re.Stop();
        }
        repeatingEvents.Clear();
    }

    protected void Loop(object token) {
        wait.Reset();
        while(!((CancellationToken)token).IsCancellationRequested) {
            bool event_ran = Process();
            if(!event_ran) {
                wait.Wait(200);
                wait.Reset();
            }
        }
    }

    public override void Do(IEventBase thisEvent) {
        base.Do(thisEvent);
        wait.Set();
    }

    public override void DoRepeating(RepeatingEvent thisEvent) {
        // enqueues repeating event at set intervals. If timer isn't
        // stopped, stopping processing thread will still stop execution
        // of events. Timers should only be created if running,
        // otherwise their behavior isn't well defined

        if(Running()) {
            base.DoRepeating(thisEvent);
        } else {
            throw new Exception("Can't enqueue an event to a non running Loop");
        }
    }
}