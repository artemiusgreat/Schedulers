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
      //AsyncPump.Run(async () =>
      //{
      //  AsyncPump.Log("in task before Task.Run");

      //  var t = Task.Run(() =>
      //  {
      //    AsyncPump.Log("in task.run()");

      //  });

      //  AsyncPump.Log("in task before await t");
      //  //t.Wait();
      //  await t;

      //  //await Task.Yield();
      //  //await Task.Yield();

      //  AsyncPump.Log("in task before await Task.Delay");

      //  await Task.Delay(10);

      //  AsyncPump.Log("Hello World!");
      //});

      var ctx = new AsyncPump.SingleThreadSynchronizationContext();
      var pump = new AsyncPump(ctx);
      var sc = new SynchronizationContextTaskScheduler(ctx);

      Log("A1");
      var queue = new BackgroundProcessor(1);

      Log("A2");
      var id1 = queue.Run(() => Log("B")).Result;

      Log("A3");
      pump.Run(async () =>
      {
        Log("O1");

        pump.Run<int>(() =>
        {
          Log("O2");
          return Task.FromResult(0);
        });

        return await Task.Run(() => Log("O3"));
      });

      Parallel.For(0, 100, o =>
      {
        //pump.Run(async () =>
        //{
        //  Log("O1");
        //  return await Task.Run(() => Log("O2"));
        //});

        //var id2 = queue.Run(() => Log("C")).Result;

        //if (id1 != id2)
        //{
        //  throw new Exception($"SYNC : { id1 } vs { id2 }");
        //}
      });

      ctx.Complete();

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