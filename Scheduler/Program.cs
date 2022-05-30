using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace Demo
{
  class Program
  {
    public static async Task Main(string[] args)
    {
      Log("A1");
      var queue = new BackgroundProcessor(1);
      var pump = new AsyncPumpContext();

      Log("A2");
      var id1 = queue.Run(() => Log("B")).Result;

      Log("A3");
      await pump.Run(() => Log("O1"));

      Parallel.For(0, 100, async o =>
      {
        await pump.Run(() => Log("O1"));
        //AsyncPump.Run(() => Task.FromResult(Log("O1")));
      });

      Log("A4");
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