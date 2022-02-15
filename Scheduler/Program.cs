using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

var o = 0;
var queue = new SyncQueue(Environment.ProcessorCount);
var pump = new AsyncPump();
var singleScheduler = new SingleScheduler { Count = 1 }.Setup();
var syncScheduler = new SyncScheduler { Count = 1 }.Setup();

var stopWatch = new Stopwatch();

stopWatch.Start();

for (var i = 0; i < 10000000; i++)
{
  //var response = await queue.Run(Task.FromResult<int>(o));
  //var response = pump.Run(() => Task.FromResult(o));
  //var response = await singleScheduler.Run(() => o);
  var response = await syncScheduler.Run(Task.FromResult<int>(o));
  o++;
}

stopWatch.Stop();

var ts = stopWatch.Elapsed;
var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

Console.WriteLine($"RunTime => { o } => { elapsedTime }");

//static int ShowThread(int counter = 0)
//{
//  var msg = string.Empty;
//  var thread = Thread.CurrentThread;

//  msg = string.Format("Counter: {0}\n", counter) +
//        string.Format("Background: {0}\n", thread.IsBackground) +
//        string.Format("Thread Pool: {0}\n", thread.IsThreadPoolThread) +
//        string.Format("Thread ID: {0}\n", thread.ManagedThreadId);

//  Console.WriteLine(msg);

//  return counter;
//}