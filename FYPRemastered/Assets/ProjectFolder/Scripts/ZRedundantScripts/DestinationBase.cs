using ProjectDawn.Navigation.Hybrid;
using UnityEngine;
using UnityEngine.AI;

public abstract class DestinationBase
{
    //protected readonly Rigidbody _rb;
    protected NavMeshPath _path;
    protected AgentAuthoring _agent;

    public DestinationBase(/*Rigidbody rb*//*NavMeshAgent*/AgentAuthoring agent)
    {
        _agent = agent;
        //_rb = rb;
        _path = new NavMeshPath();
    }

    public virtual Vector3 GetWaypointPositionOnNavMesh() => Vector3.zero;

    public abstract bool TryGetPath(out Vector3 destination);
    public virtual void Init() { }

    protected bool HasClearPathToTarget(Vector3 from, Vector3 to, out NavMeshPath path, out Vector3 destination)
    {
        path = null;
        destination = Vector3.zero;
        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, _path))
            return false;

        path = _path;
        destination = to;
        return path.status == NavMeshPathStatus.PathComplete;
    }

    protected Vector3 GetNearestPointOnNavMesh(Vector3 samplePoint, float maxDistance)
    {
        if (NavMesh.SamplePosition(samplePoint, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return _agent.transform.position;
    }
}