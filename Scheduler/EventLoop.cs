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

  protected BlockingCollection<dynamic> _items = new();

  public async void CreateLoop()
  {
    int i = 0;

    while (true)
    {
      await Task.Delay(TimeSpan.FromSeconds(1));

      //if (_inputs.TryDequeue(out Input<dynamic> input))
      //{
      //  Console.WriteLine($"Demo { Thread.CurrentThread.ManagedThreadId } { Thread.CurrentThread.IsBackground } { Thread.CurrentThread.Priority }");
      //  input.Response.SetResult(input.Action());
      //}

      i += 1;
      Console.WriteLine($"Count { i }");
    }
  }

  public Task<dynamic> Run<T>(Func<T> action)
  {
    var response = new TaskCompletionSource<dynamic>();
    var process = () => action() as dynamic;
    var input = new Input<dynamic>
    {
      Action = process,
      Response = response
    };

    _inputs.Enqueue(input);

    return response.Task;
  }
}