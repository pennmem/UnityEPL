using System;
using System.Diagnostics;
using System.Threading;

public class WorkItemState : IDisposable
{
    public Stopwatch sw { get; }
    public ManualResetEventSlim flag { get; }
    public CancellationToken cancel { get; }
    public Int32 dueTime { get; }
    public Int32 period { get; }

   public WorkItemState(Int32 _dueTime, Int32 _period, CancellationToken _cancel)
   {
       dueTime = _dueTime;
       period = _period; 
       flag = new ManualResetEventSlim();
       flag.Reset();
       sw = new Stopwatch();   
       cancel = _cancel;
   }

   public void Dispose()
   {
       flag.Set();
       flag.Dispose();
   }
}

public class HighPrecisionTimer : IDisposable
{
    private WorkItemState workState;
    private object callbackState;
    private System.Threading.WaitCallback callback;
    private object locker = new Object();
    private CancellationTokenSource tokenSource = new CancellationTokenSource();

    public HighPrecisionTimer(System.Threading.WaitCallback _callback, object _stateInfo, Int32 _dueTime, Int32 _period)
    {
        workState = new WorkItemState(_dueTime, _period, tokenSource.Token);
        callbackState = _stateInfo;
        callback = _callback;

        workState.flag.Reset();
        ThreadPool.QueueUserWorkItem(Spinner, workState);
    }

    private void Spinner(object stateInfo)
    {
        WorkItemState state = (WorkItemState) stateInfo;
        state.sw.Start();
        Int32 remainingWait = state.dueTime;

        state.flag.Reset();
        if(state.dueTime < 0)
        {
            return;
        }

        while(!state.cancel.IsCancellationRequested) { 
            state.sw.Restart();

            if(remainingWait > 0) {
                state.flag.Reset();
                state.flag.Wait(remainingWait);
                remainingWait = remainingWait - (Int32)state.sw.ElapsedMilliseconds;
            }

            if(remainingWait <= 0) {
                if (!ThreadPool.QueueUserWorkItem(callback, callbackState))
                {
                    throw new Exception("Failed to queue callback");
                }
                if(state.period >= 0) {
                    remainingWait = state.period + remainingWait;
                }
            }
        }
    }

    public bool Change(Int32 _dueTime, Int32 _period) {
        // Cancel and dispose of old state
        tokenSource.Cancel();
        tokenSource.Dispose();
        workState.Dispose();
        
        // Set up new spinner with new cancellation and flag states
        tokenSource = new CancellationTokenSource();
        workState = new WorkItemState(_dueTime, _period, tokenSource.Token);
        return ThreadPool.QueueUserWorkItem(Spinner, workState);
    }

    public void Dispose()
    {
        tokenSource.Cancel();
        tokenSource.Dispose();
        workState.Dispose();
    }
}