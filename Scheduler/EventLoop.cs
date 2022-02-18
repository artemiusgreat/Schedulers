using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

public class EventLoop
{
  SynchronizationContext _syncCtx = new();
  ConcurrentQueue<Input<dynamic>> _inputs = new();

  class Input<T>
  {
    public Func<T> Action;
    public TaskCompletionSource<T> Response;
  }

  public EventLoop()
  {
    Observable
      .Interval(TimeSpan.FromMilliseconds(1), new EventLoopScheduler())
      .Subscribe(async o =>
      {
        if (_inputs.TryDequeue(out Input<dynamic> input))
        {
          Console.WriteLine($"Demo { Thread.CurrentThread.ManagedThreadId } { Thread.CurrentThread.IsBackground } { Thread.CurrentThread.Priority }");
          input.Response.SetResult(input.Action());
        }
      });
  }

  public Task<dynamic> Run<T>(Func<T> action)
  {
    var response = new TaskCompletionSource<dynamic>();
    var process = () => Task.action() as dynamic;
    var input = new Input<dynamic>
    {
      Action = process,
      Response = response
    };

    _inputs.Enqueue(input);

    return response.Task;
  }
}