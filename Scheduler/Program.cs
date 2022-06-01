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

      Log("B");
      queue.Run(() => Log("B1"));
      queue.Run(() => Task.FromResult(Log("B2")));
      pump.Run(() => Log("B3"));
      AsyncPump.Run(() => Task.FromResult(Log("B4")));

      Log("C");
      Parallel.For(0, 10, o =>
      {
        //queue.Run(() => Log("C1"));
        //pump.Run(() => Log("C2"));
        //AsyncPump.Run(() => Task.FromResult(Log("C3")));
      });

      Log("D");
      Console.ReadKey();
    }

    public static int Log(string txt)
    {
      var message = $"{txt} - { Thread.CurrentThread.ManagedThreadId } { Thread.CurrentThread.IsBackground } { Thread.CurrentThread.Priority }";
      Console.WriteLine(message);
      return Thread.CurrentThread.ManagedThreadId;
    }
  }
}