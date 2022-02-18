using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

var demo = new Demo();
//var queue = new SyncQueue(Environment.ProcessorCount);
var pump = new AsyncPump();
//var singleScheduler = new SingleScheduler { Count = 1 }.Setup();
//var syncScheduler = new SyncScheduler { Count = 1 }.Setup();
var contextQueue = new ContextQueue();
var mainThreadId = Thread.CurrentThread.ManagedThreadId;
//var stopWatch = new Stopwatch();

//stopWatch.Start();

await contextQueue.Run(() => demo.Execute(1), SynchronizationContext.Current);

Func<string> log = () => $"{ Thread.CurrentThread.ManagedThreadId } { Thread.CurrentThread.IsBackground } { Thread.CurrentThread.Priority }";

Console.WriteLine($"Context Thread #1 is { log() }");
var ec = ExecutionContext.Capture();
Console.WriteLine($"Context Thread #2 is { log() }");

new Thread(() =>
{
  Thread.CurrentThread.IsBackground = true; 
  Console.WriteLine($"Context Thread #3 is { log() }");
  ExecutionContext.Run(ec, inputState => Console.WriteLine($"Context Thread #4 is { log() }"), null);
  Console.WriteLine($"Context Thread #5 is { log() }");

  var response = pump.Run(() =>
  {
    Console.WriteLine($"@@@@@ Main thread should be { mainThreadId } but it is { log() }");
    return Task.FromResult(demo.Execute(mainThreadId));
  });

}).Start();
Thread.Sleep(5000);
Console.WriteLine($"Context Thread #6 is { log() }");

//for (var i = 0; i < 1000000; i++)
//{
//  //var response = await queue.Run(Task.FromResult<int>(o));
//  //var response = pump.Run(() => Task.FromResult(o));
//  //var response = await singleScheduler.Run(() => o);
//  //var response = await syncScheduler.Run(() => o);
//  //var response = await syncScheduler.Run(Task.FromResult<int>(o));
//  o++;
//}

//stopWatch.Stop();

//var ts = stopWatch.Elapsed;
//var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

//Console.WriteLine($"RunTime => { o } => { elapsedTime }");

public class Demo
{
  public int Execute(int id)
  {
    Func<string> log = () => $"{ Thread.CurrentThread.ManagedThreadId } { Thread.CurrentThread.IsBackground } { Thread.CurrentThread.Priority }";

    Console.WriteLine($"@@@@@ Main thread inside callback should be { id } but it is { log() }");

    return 0;
  }
}
