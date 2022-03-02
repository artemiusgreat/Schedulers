using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class ChannelScheduler
{
  Channel<int> myChannel = Channel.CreateUnbounded<int>();
  Func<string> log = () => $"{ Thread.CurrentThread.ManagedThreadId } { Thread.CurrentThread.IsBackground } { Thread.CurrentThread.Priority }";

  public void CreateWriter()
  {
    _ = Task.Factory.StartNew(async () =>
    {
      for (int i = 0; i < 10; i++)
      {
        await myChannel.Writer.WriteAsync(i);
      }

      myChannel.Writer.Complete();
    });
  }

  public async void CreateReader()
  {
    await foreach (var item in myChannel.Reader.ReadAllAsync())
    {
      Console.WriteLine($"Read { item } { log() }");
      await Task.Delay(1000);
    }
  }
}