using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class SyncScheduler : TaskScheduler, IDisposable
{
  protected class Item<T>
  {
    public Func<T> Action { get; set; }
    public Task<T> Promise { get; set; }
    public TaskCompletionSource<T> Completion { get; set; }
    public CancellationTokenSource Cancellation { get; set; }
  }

  protected BlockingCollection<dynamic> _items = new();

  public int Count { get; set; } = 1;
  public CancellationTokenSource Cancellation { get; set; } = new();

  public virtual SyncScheduler Setup()
  {
    Parallel.For(0, Count, i => Task.Factory.StartNew(Consume, Cancellation.Token));

    return this;
  }

  public virtual Task<T> Run<T>(Func<T> action, CancellationTokenSource cancellation = null)
  {
    var item = new Item<T>
    {
      Action = action,
      Cancellation = cancellation,
      Completion = new TaskCompletionSource<T>()
    };

    _items.Add(item);

    return item.Completion.Task;
  }

  public virtual Task<T> Run<T>(Task<T> action, CancellationTokenSource cancellation = null)
  {
    var item = new Item<T>
    {
      Promise = action,
      Cancellation = cancellation,
      Completion = new TaskCompletionSource<T>()
    };

    _items.Add(item);

    return item.Completion.Task;
  }

  public void Dispose()
  {
    _items.CompleteAdding();
  }

  protected virtual void Consume()
  {
    foreach (dynamic item in _items.GetConsumingEnumerable())
    {
      var completion = item?.Completion;
      var cancellation = item?.Cancellation?.Token ?? CancellationToken.None;

      try
      {
        if (cancellation.IsCancellationRequested)
        {
          completion.SetCanceled();
          continue;
        }

        switch (true)
        {
          case true when item.Action is not null: completion.SetResult(item.Action()); break;
          case true when item.Promise is not null: completion.SetResult(item.Promise.GetAwaiter().GetResult()); break;
        }
      }
      catch (OperationCanceledException)
      {
        completion.SetCanceled();
      }
      catch (Exception e)
      {
        completion.SetException(e);
      }
    }
  }

  protected override void QueueTask(Task action) {}

  protected override IEnumerable<Task> GetScheduledTasks() => null;

  protected override bool TryExecuteTaskInline(Task item, bool isDone) => false;
}