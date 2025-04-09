using System;
using UnityEngine;
using UnityEngine.AI;

public struct PathCheckRequest
{
    public Vector3 From;
    public Vector3 To;
    public NavMeshPath Path;
    public Action<float> OnComplete;
}
