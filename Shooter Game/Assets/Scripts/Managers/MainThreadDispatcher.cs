using System;
using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : Singleton<MainThreadDispatcher>
{
    private static readonly CustomQueue<Action> executionQueue = new CustomQueue<Action>();
    void Update()
    {
        while (executionQueue.Count > 0)
        {
            executionQueue.Dequeue().Invoke();
        }
    }
    public void EnqueueAction(Action action)
    {
        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }
    }
}
