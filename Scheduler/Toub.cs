using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Stephen Toub - Synchronization context in console apps
// https://devblogs.microsoft.com/pfxteam/await-synchronizationcontext-and-console-apps/

public class AsyncPump
{
  SingleThreadSynchronizationContext _syncCtx = new();

  public T Run<T>(Func<T> action)
  {
    var prevCtx = SynchronizationContext.Current;

    try
    {
      SynchronizationContext.SetSynchronizationContext(_syncCtx);

      var response = action();

      _syncCtx.Complete();
      _syncCtx.Run();

      return response;
    }
    finally
    {
      SynchronizationContext.SetSynchronizationContext(prevCtx);
    }
  }

  public T Run<T>(Task<T> action)
  {
    var prevCtx = SynchronizationContext.Current;

    try
    {
      SynchronizationContext.SetSynchronizationContext(_syncCtx);

      action.ContinueWith(o => _syncCtx.Complete(), TaskScheduler.Default);

      _syncCtx.Run();

      return action.GetAwaiter().GetResult();
    }
    finally
    {
      SynchronizationContext.SetSynchronizationContext(prevCtx);
    }
  }
}

public class SingleThreadSynchronizationContext : SynchronizationContext
{
  protected BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue = new();

  public override void Post(SendOrPostCallback d, object state)
  {
    _queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
  }

  public void Run()
  {
    foreach (var workItem in _queue.GetConsumingEnumerable())
    {
      workItem.Key(workItem.Value);
    }
  }

  public void Complete()
  {
    _queue.CompleteAdding();
  }
}