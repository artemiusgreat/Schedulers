using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace Demo
{
  class Program
  {
    public static void Main(string[] args)
    {
      Log("A");
      var sc = new CurrentThreadTaskScheduler();
      var queue = new BackgroundProcessor();
      var pump = new AsyncPumpContext();

      SynchronizationContext.SetSynchronizationContext(pump._syncCtx);

      Log("B");
      queue.Run(() => Log("B1"));
      queue.Run(() => Task.FromResult(Log("B2")));
      pump.Run(() => Task.Run(() => Log("B3")));
      AsyncPump.Run(() => Task.Run(() => Log("B4")));

      var scheduler = new ConcurrentExclusiveSchedulerPair();

      Task.Factory.StartNew(() => Log("E"), 
        CancellationToken.None, 
        TaskCreationOptions.DenyChildAttach, 
        scheduler.ExclusiveScheduler);

      Log("C");
      Parallel.For(0, 5, o =>
      {
        pump.Run(() => Run("B3"));
        AsyncPump.Run(() => Run("B4"));

        Task.Factory.StartNew(() => Log("E"),
          CancellationToken.None,
          TaskCreationOptions.DenyChildAttach,
          scheduler.ExclusiveScheduler);

        queue.Run(() => Task.Run(() => Log("C1")));
        //pump.Run(() => Log("C2"));
        //AsyncPump.Run(() => Task.FromResult(Log("C3")));
      });

      Log("D");
      Console.ReadKey();
    }

    public static Task<int> Run(string name)
    {
      return Task.Factory.StartNew(
        () => Log(name),
        CancellationToken.None,
        TaskCreationOptions.None,
        TaskScheduler.FromCurrentSynchronizationContext());
    }

    public static int Log(string txt)
    {
      var message = $"{txt} - {Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.IsBackground} {Thread.CurrentThread.Priority}";
      Console.WriteLine(message);
      return Thread.CurrentThread.ManagedThreadId;
    }
  }
}