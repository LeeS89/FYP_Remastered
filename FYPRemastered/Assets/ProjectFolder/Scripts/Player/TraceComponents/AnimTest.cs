using UnityEngine;
using UnityEngine.AI;

public class AnimTest : MonoBehaviour
{
    public Transform _cube;
    public NavMeshAgent _agent;

    private void Start()
    {
        _agent.SetDestination(_cube.position);
    }
}
