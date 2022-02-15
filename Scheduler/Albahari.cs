using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class SyncQueue : IDisposable
{
  protected class Item<T>
  {
    public Task<T> Action { get; set; }
    public TaskCompletionSource<T> Completion { get; set; }
    public CancellationTokenSource Cancellation { get; set; }
  }

  protected BlockingCollection<dynamic> _items = new();
  protected CancellationTokenSource _cancellation = new();

  public SyncQueue(int count)
  {
    Parallel.For(0, count, i => Task.Factory.StartNew(Consume, _cancellation.Token));
  }

  public virtual void Dispose()
  {
    _items.CompleteAdding();
    _cancellation.Cancel();
  }

  public virtual Task<T> Run<T>(Task<T> action, CancellationTokenSource cancellation = null)
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

  protected virtual void Consume()
  {
    foreach (dynamic item in _items.GetConsumingEnumerable())
    {
      var response = item?.Completion;
      var cancellation = item?.Cancellation?.Token ?? CancellationToken.None;

      try
      {
        if (cancellation.IsCancellationRequested)
        {
          response.SetCanceled();
          continue;
        }

        response.SetResult(item.Action.GetAwaiter().GetResult());
      }
      catch (OperationCanceledException)
      {
        response.SetCanceled();
      }
      catch (Exception e)
      {
        response.SetException(e);
      }
    }
  }
}