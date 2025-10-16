using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskScheduler : MonoBehaviour 
{
    public static TaskScheduler Instance { get; private set; }

    private List<int> _ticks = new();
    private List<float> _timeBetweenTicks = new();
    private List<Action> _callbacks = new();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void Schedule(TaskContext context)
    {
        _ticks.Add(context.NumTicks);
        _timeBetweenTicks.Add(context.TimeBetweenTicks);
        _callbacks.Add(context.Callback);
    }

    private void Update()
    {
        
    }
}
