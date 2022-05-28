using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
  /// <summary>
  // Luaan - Single threaded scheduler - https://stackoverflow.com/a/30726903/437393
  /// </summary>
  public sealed class StaScheduler : TaskScheduler
  {
    [ThreadStatic]
    private static bool _isExecuting;
    private readonly CancellationToken _cancellation;
    private readonly BlockingCollection<Task> _queue;

    public StaScheduler(CancellationToken cancellation)
    {
      _cancellation = cancellation;
      _queue = new BlockingCollection<Task>();
    }

    public void Start() => new Thread(Run) { Name = "Demo" }.Start();

    // Just a helper for the sample code
    public Task Schedule(Action action) => Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, this);

    // You can have this public if you want - just make sure to hide it
    public void Run()
    {
      _isExecuting = true;

      try
      {
        foreach (var task in _queue.GetConsumingEnumerable(_cancellation))
        {
          TryExecuteTask(task);
        }
      }
      catch (OperationCanceledException)
      { }
      finally
      {
        _isExecuting = false;
      }
    }

    // Signaling this allows the task scheduler to finish after all tasks complete
    public void Complete() => _queue.CompleteAdding();
    protected override IEnumerable<Task> GetScheduledTasks() => null;

    protected override void QueueTask(Task item)
    {
      try
      {
        _queue.Add(item, _cancellation);
      }
      catch (OperationCanceledException) { }
    }

    protected override bool TryExecuteTaskInline(Task task, bool isDone)
    {
      // We'd need to remove the task from queue if it was already queued. 
      // That would be too hard.
      return isDone is false && _isExecuting && TryExecuteTask(task);
    }
  }
}