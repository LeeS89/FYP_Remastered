using System;
using UnityEngine;
using UnityEngine.AI;

public class PathRequest
{
    public Vector3 start;
    public Vector3 end;
    public NavMeshPath path;
    public Action<bool> callback;
}