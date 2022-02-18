using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Nirmalya Bhattacharyya - Sync Context over Execution Context
// https://nirmalyabhattacharyya.wordpress.com/2013/08/31/executioncontext-synchronizationcontext-and-callcontext/ 

public class ContextQueue
{
  //SingleThreadSynchronizationContext _syncCtx = new();
  SynchronizationContext _syncCtx = new();

  public Task<T> Run<T>(Func<T> action, SynchronizationContext syncCtx)
  {
    _syncCtx = syncCtx ?? _syncCtx;

    var state = "State A";
    var data = "Data A";
    var dataLocal = new AsyncLocal<string>();
    var response = new TaskCompletionSource<T>();
    var storage = new Dictionary<string, string>();

    dataLocal.Value = "Local A";
    storage["x"] = "Hash A";

    //Just before we start processing on a new thread 
    //the current thread’s execution environment is copied
    //var ec = ExecutionContext.Capture();

    ThreadPool.QueueUserWorkItem((inputs) =>    
    {
      //And subsequently passed in to create the new execution environment
      //ExecutionContext.Run(ec, inputState =>
      //{
      //  var res = action();

      //  //Execution environment for executing code 
      //  //is set by the passed in ExecutionContext instance
      //  //Console.WriteLine($"{ res } vs { state } vs { inputState } vs { data } vs { dataLocal.Value } vs { storage["x"] }");

      //  //response.SetResult(res);

      //}, state);

      _syncCtx.Post(o => 
      {
        Console.WriteLine($"Demo { Thread.CurrentThread.ManagedThreadId } { Thread.CurrentThread.IsBackground } { Thread.CurrentThread.Priority }");

        var res = action();
        response.SetResult(res);
      }, null);

    });

    state = "State B";
    data = "Data B";
    dataLocal.Value = "Local B";
    storage["x"] = "Hash B";

    return response.Task;
  }
}