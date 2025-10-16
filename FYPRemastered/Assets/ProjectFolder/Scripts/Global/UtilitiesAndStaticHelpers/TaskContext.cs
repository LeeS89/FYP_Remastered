using System;
using UnityEngine;

public struct TaskContext
{
    public int NumTicks;
    public float TimeBetweenTicks;
    public Action Callback;
}
