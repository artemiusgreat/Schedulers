﻿using System;
using System.Collections.Concurrent;
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
    public class Item<T>
    {
      public Delegate Action { get; set; }
      public TaskCompletionSource<T> Completion { get; set; }
      public CancellationTokenSource Cancellation { get; set; }
    }

    protected BlockingCollection<Item<dynamic>> _queue = new();

    public BackgroundProcessor(int count = 1, TaskScheduler scheduler = null, CancellationTokenSource source = null)
    {
      var sc = scheduler ?? TaskScheduler.Default;
      var cancellation = source?.Token ?? CancellationToken.None;

      for (var i = 0; i < count; i++)
      {
        Task.Factory.StartNew(Consume, cancellation, TaskCreationOptions.LongRunning, sc);
      }
    }

    public virtual void Dispose()
    {
      //if (_channel.Writer.TryComplete())
      //{
      //  _channel.Reader.Completion.Dispose();
      //}
    }

    public virtual Task<dynamic> Run(Func<dynamic> action, CancellationTokenSource cancellation = null)
    {
      var item = Create(action, cancellation);

      _queue.Add(item);

      return item.Completion.Task;
    }

    public virtual Item<dynamic> Create(Func<dynamic> action, CancellationTokenSource cancellation = null)
    {
      var item = new Item<dynamic>
      {
        Action = action,
        Cancellation = cancellation,
        Completion = new TaskCompletionSource<dynamic>()
      };

      return item;
    }

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