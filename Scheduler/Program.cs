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
      var sc = new StaScheduler(CancellationToken.None);

      Log("A1");
      var queue = new BackgroundProcessor(1, sc);

      Log("A2");
      var id1 = queue.Run<dynamic>(() => Log("B")).Result;

      Log("A3");
      Parallel.For(0, 1000, o =>
      {
        var id2 = queue.Run<dynamic>(() => Log("C")).Result;

        if (id1 != id2)
        {
          throw new Exception($"SYNC : { id1 } vs { id2 }");
        }
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