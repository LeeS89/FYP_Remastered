using System;
using UnityEngine;
using UnityEngine.AI;

[Obsolete("Former Test script", true)]
public class AnimTest : MonoBehaviour
{
    public Transform _cube;
    public NavMeshAgent _agent;

    private void Start()
    {
        _agent.SetDestination(_cube.position);
    }
}
