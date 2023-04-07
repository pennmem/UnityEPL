using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnityEPL {

    // Origin: https://stackoverflow.com/a/30726903
    // If something more complex is needed in the future, look at the following links
    // https://devblogs.microsoft.com/pfxteam/parallelextensionsextras-tour-7-additional-taskschedulers/
    // https://github.com/ChadBurggraf/parallel-extensions-extras/tree/master/TaskSchedulers
    public sealed class SingleThreadTaskScheduler : TaskScheduler {
        [ThreadStatic]
        private static bool _isExecuting;

        private readonly CancellationToken _cancellationToken;

        private readonly BlockingCollection<Task> _taskQueue;

        private readonly Thread _singleThread;
        public override int MaximumConcurrencyLevel => 1;

        public SingleThreadTaskScheduler(CancellationToken cancellationToken) {
            this._cancellationToken = cancellationToken;
            this._taskQueue = new BlockingCollection<Task>();
            _singleThread = new Thread(RunOnCurrentThread) { Name = "STTS Thread", IsBackground = true };
            _singleThread.Start();
        }

        private void RunOnCurrentThread() {
            _isExecuting = true;

            try {
                foreach (var task in _taskQueue.GetConsumingEnumerable(_cancellationToken)) {
                    TryExecuteTask(task);
                }
            } catch (OperationCanceledException) {
                // TODO: JPB: (feature) Add catch handler for Tasks that throw and exception
                //            Send them to an error notification in the task
                //            Also, look at this link: https://www.codeguru.com/csharp/async-methods-exception-handling/
            } finally {
                _isExecuting = false;
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks() => _taskQueue.ToList();

        protected override void QueueTask(Task task) {
            try {
                _taskQueue.Add(task, _cancellationToken);
            } catch (OperationCanceledException) {
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
            // We'd need to remove the task from queue if it was already queued. 
            // That would be too hard.
            if (taskWasPreviouslyQueued)
                return false;

            return _isExecuting && TryExecuteTask(task);
        }
    }

}