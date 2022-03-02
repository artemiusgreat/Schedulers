using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

class Program
{
  SynchronizationContext ctx = null;
  SynchronizationContextScheduler ctxScheduler = null;
  EventLoopScheduler eventScheduler = null;
  SyncScheduler syncScheduler = null;

  static void Main(string[] args)
  {
    new Program().Demo();
  }

  void Demo()
  {
    ctx = SynchronizationContext.Current ?? new SynchronizationContext();
    ctxScheduler = new SynchronizationContextScheduler(ctx);
    eventScheduler = new EventLoopScheduler();
    syncScheduler = new SyncScheduler();

    SynchronizationContext.SetSynchronizationContext(ctx);

    Task.Factory.StartNew(() =>
    {
      Func<string> log = () => $"{ Thread.CurrentThread.ManagedThreadId } { Thread.CurrentThread.IsBackground } { Thread.CurrentThread.Priority }";

      Console.WriteLine($"Start { log() }");

      //var channels = new ChannelScheduler();

      //channels.CreateWriter();
      //channels.CreateReader();

      var rx = new RxScheduler();

      rx.CreateSubscription();
      rx.Send(5);

      //var aTimer = new System.Timers.Timer(1000);
      //aTimer.AutoReset = false;
      //aTimer.Enabled = true;
      //aTimer.Elapsed += (o, e) => 
      //{
      //  Console.WriteLine($"Time { log() }");
      //  aTimer.Enabled = true;
      //};

      Console.WriteLine($"Done");
      Console.ReadLine();

      ////var demo = new Demo();
      ////var queue = new SyncQueue(Environment.ProcessorCount);
      //var pump = new AsyncPump();
      ////var singleScheduler = new SingleScheduler { Count = 1 }.Setup();
      //var syncScheduler = new SyncScheduler { Count = 1 }.Setup();
      //var contextQueue = new ContextQueue();
      //var eventLoop = new EventLoop();
      //var mainThreadId = Thread.CurrentThread.ManagedThreadId;
      //var stopWatch = new Stopwatch();

      //stopWatch.Start();

      //Console.WriteLine($"Context Thread #1 is { log() }");

      ////await contextQueue.Run(() => demo.Execute(1), SynchronizationContext.Current);

      ////var ec = ExecutionContext.Capture();
      ////Console.WriteLine($"Context Thread #2 is { log() }");

      ////eventLoop.CreateLoop();

      ////var data = await eventLoop.Run(() =>
      ////{
      ////  Console.WriteLine($"@@@@@ Main thread A should be { mainThreadId } but it is { log() }");
      ////  return demo.Execute(1);
      ////});

      ////new Thread(async () =>
      ////{
      ////  Thread.CurrentThread.IsBackground = true; 
      ////  Console.WriteLine($"Context Thread #3 is { log() }");
      ////  ExecutionContext.Run(ec, inputState => Console.WriteLine($"Context Thread #4 is { log() }"), null);
      ////  Console.WriteLine($"Context Thread #5 is { log() }");

      ////  var response = pump.Run(() =>
      ////  {
      ////    Console.WriteLine($"@@@@@ Main thread B should be { mainThreadId } but it is { log() }");
      ////    return demo.Execute(mainThreadId);
      ////  });

      ////}).Start();
      ////Thread.Sleep(5000);

      //for (var i = 0; i < 1000000; i++)
      //{
      //  //var response = await queue.Run(Task.FromResult<int>(o));
      //  //var response = pump.Run(() => Task.FromResult(o));
      //  //var response = await singleScheduler.Run(() => o);
      //  var response = await syncScheduler.Run(() => i);
      //  //var response = await syncScheduler.Run(Task.FromResult<int>(o));
      //}

      //stopWatch.Stop();

      //var ts = stopWatch.Elapsed;
      //var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

      //Console.WriteLine($"RunTime => { elapsedTime }");

      //public class Demo
      //{
      //  public int Execute(int id)
      //  {
      //    Func<string> log = () => $"{ Thread.CurrentThread.ManagedThreadId } { Thread.CurrentThread.IsBackground } { Thread.CurrentThread.Priority }";

      //    Console.WriteLine($"@@@@@ Main thread inside callback should be { id } but it is { log() }");

      //    return 0;
      //  }
      //}

    }, CancellationToken.None, TaskCreationOptions.None, syncScheduler);

    Console.ReadLine();
  }
}