using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Single threaded scheduler with Task.Factory.StartNew
// https://stackoverflow.com/a/30726903/437393

public class SingleScheduler : TaskScheduler, IDisposable
{
  protected bool isExecuting = false;
  protected BlockingCollection<Task> _items = new();

  public int Count { get; set; } = 1;
  public CancellationTokenSource Cancellation { get; set; } = new();

  public virtual SingleScheduler Setup()
  {
    Parallel.For(0, Count, i => Task.Factory.StartNew(Consume, Cancellation.Token));

    return this;
  }

  public virtual Task<T> Run<T>(Func<T> action, CancellationTokenSource cancellation = null)
  {
    var process = Task.Factory.StartNew(
      action,
      cancellation?.Token ?? CancellationToken.None,
      TaskCreationOptions.None,
      this);

    return process;
  }

  protected virtual void Consume()
  {
    isExecuting = true;

    try
    {
      foreach (var item in _items.GetConsumingEnumerable(Cancellation.Token))
      {
        TryExecuteTask(item);
      }
    }
    catch (OperationCanceledException)
    {
    }
    finally
    {
      isExecuting = false;
    }
  }

  protected override IEnumerable<Task> GetScheduledTasks()
  {
    return null;
  }

  protected override void QueueTask(Task task)
  {
    try
    {
      _items.Add(task, Cancellation.Token);
    }
    catch (OperationCanceledException)
    {
    }
  }

  protected override bool TryExecuteTaskInline(Task item, bool isDone)
  {
    return isDone == false && isExecuting && TryExecuteTask(item);
  }

  public void Dispose()
  {
    _items.CompleteAdding();
  }
}