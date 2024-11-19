using System;
using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ThreadManager : Singleton<ThreadManager>
{
    public List<Thread> activeThreads;

    private void Awake()
    {
        activeThreads = new List<Thread>();
    }
    private void OnApplicationQuit()
    {
        if (activeThreads != null && activeThreads.Count > 0)
        {
            foreach (Thread thread in activeThreads)
            {
                if (thread != null && thread.IsAlive)
                {
                    thread.Abort();
                }
            }
        }
    }
}
