using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
  /// <summary>
  /// Stephen Toub - Synchronization Context - https://devblogs.microsoft.com/pfxteam/await-synchronizationcontext-and-console-apps/
  /// </summary>
  public class AsyncPump
  {
    public SingleThreadSynchronizationContext _syncCtx = null;

    public AsyncPump(SingleThreadSynchronizationContext ctx = null)
    {
      SynchronizationContext.SetSynchronizationContext(_syncCtx = ctx ?? new());
    }

    public T Run<T>(Func<Task<T>> action)
    {
      var prevCtx = SynchronizationContext.Current;

      try
      {
        SynchronizationContext.SetSynchronizationContext(_syncCtx);

        var o = action();

        o.ContinueWith(o => _syncCtx.Complete(), TaskScheduler.Default);

        _syncCtx.Run();

        return o.GetAwaiter().GetResult();
      }
      finally
      {
        SynchronizationContext.SetSynchronizationContext(prevCtx);
      }
    }

    public class SingleThreadSynchronizationContext : SynchronizationContext
    {
      private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue = new();

      public override void Post(SendOrPostCallback e, object state)
      {
        _queue.Add(new KeyValuePair<SendOrPostCallback, object>(e, state));
      }

      public override void Send(SendOrPostCallback e, object state) => throw new NotSupportedException("No sync");

      public void Run()
      {
        foreach (var item in _queue.GetConsumingEnumerable())
        {
          item.Key(item.Value);
        }
      }

      public void Complete() => _queue.CompleteAdding();
    }
  }
}