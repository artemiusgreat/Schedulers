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
  public class AsyncPumpContext
  {
    public SingleThreadSynchronizationContext _syncCtx = new();

    public Task<T> Run<T>(Func<T> action)
    {
      var prevCtx = SynchronizationContext.Current;

      try
      {
        SynchronizationContext.SetSynchronizationContext(_syncCtx);
        var response = new TaskCompletionSource<T>();
        _syncCtx.Post(o => response.SetResult(action()), null);
        _syncCtx.Run();
        return response.Task;
      }
      finally
      {
        SynchronizationContext.SetSynchronizationContext(prevCtx);
      }
    }

    public class SingleThreadSynchronizationContext : SynchronizationContext
    {
      private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue = new(new ConcurrentQueue<KeyValuePair<SendOrPostCallback, object>>());
      public override void Post(SendOrPostCallback e, object state) => _queue.Add(new KeyValuePair<SendOrPostCallback, object>(e, state));
      public override void Send(SendOrPostCallback e, object state) => throw new NotSupportedException("No sync");

      public void Run()
      {
        if (_queue.TryTake(out KeyValuePair<SendOrPostCallback, object> item))
        {
          item.Key(item.Value);
        }
      }
    }
  }
}