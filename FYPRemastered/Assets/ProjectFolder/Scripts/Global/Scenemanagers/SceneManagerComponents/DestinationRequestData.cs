using System;
using UnityEngine;
using UnityEngine.AI;

public class DestinationRequestData
{
    public DestinationType destinationType;
    public Vector3 start;
    public Vector3 end;
    public NavMeshPath path;
    
    public Action<bool> internalCallback;
    public Action<bool> externalCallback;
}