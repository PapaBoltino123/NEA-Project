using System;
using System.Collections.Generic;
using System.Threading;

public class ThreadManager
{
    private List<Thread> threadList;
    private bool isRunning;

    public ThreadManager()
    {
        threadList = new List<Thread>();
        isRunning = false;
    }

    public void StartThread(Action action)
    {
        if (isRunning == false)
        {
            Thread thread = new Thread(new ThreadStart(action));
            threadList.Add(thread);
            thread.Start();
        }
    }

    public void StopAllThreads()
    {
        isRunning = false;
        foreach (var thread in threadList)
        {
            if (thread.IsAlive == true)
                thread.Join();
        }
        threadList.Clear();
        Console.WriteLine("All threads have been stopped.");
    }
    public bool AreAnyThreadsRunning()
    {
        foreach (var thread in threadList)
        {
            if (thread.IsAlive)
                return true;
        }
        return false;
    }
}

// Example usage:
class Program
{
    static void Main()
    {
        ThreadManager threadManager = new ThreadManager();

        // Start some threads
        threadManager.StartThread(() =>
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"Thread 1: {i}");
                Thread.Sleep(500);
            }
        });

        threadManager.StartThread(() =>
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"Thread 2: {i}");
                Thread.Sleep(500);
            }
        });

        // Wait a bit before stopping
        Thread.Sleep(3000);
        threadManager.StopAllThreads();
    }
}