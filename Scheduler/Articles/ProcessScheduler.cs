using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo
{
  public class ProcessScheduler : TaskScheduler
  {
    protected BlockingCollection<Task> _queue = null;
    protected BackgroundProcessor _processor = null;

    public ProcessScheduler()
    {
      _queue = new();
      _processor = new BackgroundProcessor(1);
    }

    public Task<dynamic> Run(Func<dynamic> action)
    {
      var process = _processor.Create(action);
      QueueTask(process.Completion.Task);
      return process.Completion.Task;
    }

    protected override bool TryExecuteTaskInline(Task action, bool isDone)
    {
      return false;
    }

    protected override void QueueTask(Task action) => _queue.Add(action);
    protected override IEnumerable<Task> GetScheduledTasks() => _queue;
    public override int MaximumConcurrencyLevel => 1;
  }
}
