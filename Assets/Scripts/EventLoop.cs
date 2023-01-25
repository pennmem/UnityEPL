using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class EventLoop2 : YieldedEventQueue {
    protected ManualResetEventSlim wait;
    private CancellationTokenSource tokenSource;
    private CancellationToken cancellationToken;

    public EventLoop2() {
        wait = new ManualResetEventSlim();
        running = false;
    }

    ~EventLoop2() {
        StopTimers();
        wait.Dispose();
    }

    public void Start() {
        if (IsRunning()) {
            return;
        }

        running = true;
        Thread loop = new Thread(Loop);

        tokenSource = new CancellationTokenSource();
        loop.Start(tokenSource.Token);
    }

    public void Stop() {
        if (!IsRunning()) {
            return;
        }

        running = false;
        tokenSource.Cancel();
        tokenSource.Dispose();
        wait.Set();
        StopTimers();
    }

    public void StopTimers() {
        foreach (var re in repeatingEvents) {
            re.Stop();
        }
        repeatingEvents.Clear();
    }

    protected void Loop(object token) {
        cancellationToken = (CancellationToken)token;
        wait.Reset();
        while (!cancellationToken.IsCancellationRequested) {
            bool event_ran = Process();
            if (!event_ran) {
                wait.Wait(20);
                wait.Reset();
            }
        }
    }

    public override void Do(IEnumerator thisEvent) {
        base.Do(thisEvent);
        wait.Set();
    }

    public override void DoIn(IEnumerator thisEvent, int delay) {
        base.DoIn(thisEvent, delay);
        wait.Set();
    }

    public override void DoRepeating(IEnumerator thisEvent, int iterations, int delay, int interval) {
        base.DoRepeating(thisEvent, iterations, delay, interval);
        wait.Set();
    }
    public override void DoRepeating(RepeatingEvent thisEvent) {
        base.DoRepeating(thisEvent);
        wait.Set();
    }

    public override IEnumerator DoBlocking(IEnumerator thisEvent) {
        return base.DoBlocking(thisEvent);
        // TODO: JPB: (bug) Set needs to be right before task.Wait() in EventQueue
        // wait.Set();
    }

    public override IEnumerator<T> DoGet<T>(IEnumerator thisEvent) {
        return base.DoGet<T>(thisEvent);
        // TODO: JPB: (bug) Set needs to be right before task.Wait() in EventLoop
        // wait.Set();
    }
}
