using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Demo
{
  public class RxProcessor
  {
    Func<string> log = () => $"{ Thread.CurrentThread.ManagedThreadId } { Thread.CurrentThread.IsBackground } { Thread.CurrentThread.Priority }";

    SynchronizationContext ctx = null;
    SynchronizationContextScheduler ctxScheduler = null;
    EventLoopScheduler scheduler = null;
    Subject<dynamic> observable = null;

    public RxProcessor()
    {
      ctx = SynchronizationContext.Current ?? new SynchronizationContext();
      ctxScheduler = new(ctx);
      scheduler = new();
    }

    public void CreateSubscription()
    {
      observable = new Subject<dynamic>();

      Console.WriteLine($"Before Get { log() }");

      var sub = observable.ObserveOn(ctx).Subscribe(o =>
      {
        Console.WriteLine($"Get { log() }");
      });
    }

    public void Send(int message)
    {
      Console.WriteLine($"Before Send { log() }");

      //Task.Run(() =>
      //{
      //  Console.WriteLine($"Send { log() }");
      //  observable.OnNext(message);
      //});

      observable.OnNext(message);
    }
  }
}