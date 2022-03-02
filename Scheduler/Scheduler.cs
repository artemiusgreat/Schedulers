using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
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

  protected Channel<dynamic> _channel = Channel.CreateUnbounded<dynamic>();

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

    _channel.Writer.WriteAsync(item);

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

    _channel.Writer.WriteAsync(item);

    return item.Completion.Task;
  }

  public virtual void Dispose()
  {
    _channel.Reader.Completion.Dispose();
    _channel.Writer.Complete();
  }

  protected virtual async void Consume()
  {
    await foreach (dynamic item in _channel.Reader.ReadAllAsync())
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

  protected override void QueueTask(Task action)
  {
    Run<dynamic>(() =>
    {
      action.GetAwaiter().GetResult();
      return 5;
    });
  }

  protected override IEnumerable<Task> GetScheduledTasks()
  {
      //return _channel.Reader.ReadAllAsync().ToListAsync().Result;
  }

  protected override bool TryExecuteTaskInline(Task item, bool isDone) => false;
}