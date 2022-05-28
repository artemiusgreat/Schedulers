using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
  /// <summary>
  // Theodor Zoulias - support for multi-thread scheduler with async / await - https://stackoverflow.com/a/57702536/437393 
  /// </summary>
  public class SemaphoreScheduler : TaskScheduler
  {
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

    protected async override void QueueTask(Task task)
    {
      await _semaphore.WaitAsync();

      try
      {
        await Task.Run(() => base.TryExecuteTask(task));
        await task;
      }
      finally
      {
        _semaphore.Release();
      }
    }

    protected override bool TryExecuteTaskInline(Task task, bool isDone) => false;
    protected override IEnumerable<Task> GetScheduledTasks() { yield break; }
  }
}