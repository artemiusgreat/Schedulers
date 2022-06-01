using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
  /// <summary>
  // Joe Albahari and Dedicated Process - http://www.albahari.com/threading/part5.aspx#_BlockingCollectionT
  /// </summary>

  public class BackgroundProcessor
  {
    /// <summary>
    /// Awaitable action wrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Item<T>
    {
      public Delegate Action { get; set; }
      public Task<T> Promise { get; set; }
      public TaskCompletionSource<T> Completion { get; set; }
      public CancellationTokenSource Cancellation { get; set; }
    }

    /// <summary>
    /// Queue
    /// </summary>
    protected BlockingCollection<Item<dynamic>> _queue = new(new ConcurrentQueue<Item<dynamic>>());

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="scheduler"></param>
    /// <param name="source"></param>
    public BackgroundProcessor(TaskScheduler scheduler = null, CancellationTokenSource source = null)
    {
      var sc = scheduler ?? TaskScheduler.Default;
      var cancellation = source?.Token ?? CancellationToken.None;

      Task.Factory.StartNew(Consume, cancellation, TaskCreationOptions.LongRunning, sc);
    }

    /// <summary>
    /// Action processor
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public virtual Task<dynamic> Run(Func<dynamic> action, CancellationTokenSource cancellation = null)
    {
      var item = new Item<dynamic>
      {
        Action = action,
        Cancellation = cancellation,
        Completion = new TaskCompletionSource<dynamic>()
      };

      _queue.Add(item);

      return item.Completion.Task;
    }

    /// <summary>
    /// Promise processor
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public virtual Task<dynamic> Run(Func<Task<dynamic>> action, CancellationTokenSource cancellation = null)
    {
      var item = new Item<dynamic>
      {
        Promise = action(),
        Cancellation = cancellation,
        Completion = new TaskCompletionSource<dynamic>()
      };

      _queue.Add(item);

      return item.Completion.Task;
    }

    /// <summary>
    /// Background process
    /// </summary>
    protected virtual void Consume()
    {
      foreach (var item in _queue.GetConsumingEnumerable())
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
            case true when item.Action is not null: completion.SetResult(item.Action.DynamicInvoke()); break;
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
  }
}