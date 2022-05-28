using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
  /// <summary>
  // Nirmalya Bhattacharyya - Sync Context over Execution Context - https://nirmalyabhattacharyya.wordpress.com/2013/08/31/executioncontext-synchronizationcontext-and-callcontext/ 
  /// </summary>
  public class ContextProcessor
  {
    /// <summary>
    /// Capture execution context
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    public Task<T> RunEc<T>(Func<T> action)
    {
      // Just before we start processing on a new thread 
      // the current thread’s execution environment is copied

      var ec = ExecutionContext.Capture();
      var response = new TaskCompletionSource<T>();
      var process = new Thread(inputs =>
      {
      // And subsequently passed in to create the new execution environment
      // Execution environment for executing code 
      // is set by the passed in ExecutionContext instance

      ExecutionContext.Run(ec, inputState => response.SetResult(action()), null);
      });

      return response.Task;
    }

    /// <summary>
    /// Delegate to synchronization context
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <param name="syncCtx"></param>
    /// <returns></returns>
    public Task<T> RunSc<T>(Func<T> action, SynchronizationContext syncCtx = null)
    {
      var response = new TaskCompletionSource<T>();
      var storage = new Dictionary<string, string>();
      var prevCtx = SynchronizationContext.Current;

      var process = new Thread((inputs) =>
      {
        syncCtx.Post(o => response.SetResult(action()), null);
      });

      return response.Task;
    }
  }
}