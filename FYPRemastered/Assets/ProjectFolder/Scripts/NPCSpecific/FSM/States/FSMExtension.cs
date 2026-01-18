using UnityEngine;
using UnityEngine.AI;

public static class FSMExtension
{
    public static float GetPathDistance(this NavMeshAgent agent, StateId currentstate, Vector3[] cornerBuf, float cap = float.PositiveInfinity)
    {
        if (!agent || !agent.isOnNavMesh || agent.pathPending || !agent.hasPath)
            return float.PositiveInfinity;

        if (currentstate == StateId.Patrol)
            return agent.remainingDistance;

        int n = agent.path.GetCornersNonAlloc(cornerBuf);
        if (n <= 1) return 0f;

        Vector3 pos = agent.nextPosition;
        float sum = Vector3.Distance(pos, cornerBuf[1]);
        if (sum >= cap) return cap;

        for(int i = 1; i < n - 1; i++)
        {
            sum += Vector3.Distance(cornerBuf[i], cornerBuf[i + 1]);
            if (sum >= cap) return cap;
        }

        return sum;
    }

    public static void RotateTowards(this ITargetable owner, ITargetable rotateTowards)
    {
        if (owner == null || owner.Transform == null
            || rotateTowards == null || rotateTowards.Transform == null) return;

        Transform t = owner.Transform;
        Transform target = rotateTowards.Transform;
        Vector3 toTarget = target.position - t.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.0001f) return;

        Vector3 forward = t.forward;
        forward.y = 0f;

        float dot = Vector3.Dot(forward.normalized, toTarget.normalized);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        const float precisionThreshold = 1f;
        Quaternion targetRotation = Quaternion.LookRotation(toTarget);

        if(angle < precisionThreshold)
        {
            t.rotation = Quaternion.Slerp(
                t.rotation,
                targetRotation,
                1f);
            return;
        }

        t.rotation = Quaternion.Slerp(
            t.rotation,
            targetRotation,
            Time.deltaTime * 5f);
    }
}
