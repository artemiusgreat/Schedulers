using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Demo
{
  /// <summary>
  // Joe Albahari and Dedicated Process - http://www.albahari.com/threading/part5.aspx#_BlockingCollectionT
  /// </summary>
  public class BackgroundProcessor : IDisposable
  {
    protected class Item<T>
    {
      public Delegate Action { get; set; }
      public TaskCompletionSource<T> Completion { get; set; }
      public CancellationTokenSource Cancellation { get; set; }
    }

    protected Channel<Item<dynamic>> _channel = Channel.CreateUnbounded<Item<dynamic>>();

    public BackgroundProcessor(int count = 1, TaskScheduler scheduler = null, CancellationTokenSource cancellation = null)
    {
      for (var i = 0; i < count; i++)
      {
        Task.Factory.StartNew(
          Consume,
          cancellation?.Token ?? CancellationToken.None,
          TaskCreationOptions.None,
          scheduler ?? TaskScheduler.Default);
      }
    }

    public virtual void Dispose()
    {
      if (_channel.Writer.TryComplete())
      {
        _channel.Reader.Completion.Dispose();
      }
    }

    public virtual Task<dynamic> Run<T>(Func<dynamic> action, CancellationTokenSource cancellation = null)
    {
      var item = new Item<dynamic>
      {
        Action = action,
        Cancellation = cancellation,
        Completion = new TaskCompletionSource<dynamic>()
      };

      _channel.Writer.WriteAsync(item);

      return item.Completion.Task;
    }

    protected virtual async void Consume()
    {
      await foreach (var item in _channel.Reader.ReadAllAsync())
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

          completion.SetResult(item.Action.DynamicInvoke());
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