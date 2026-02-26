using UnityEngine;
using UnityEngine.AI;

public static class FSMExtension
{
    public static float GetPathDistance(this NavMeshAgent agent, StateId currentstate, Vector3[] cornerBuf, float cap = float.PositiveInfinity)
    {
        /*if (!agent || !agent.isOnNavMesh || agent.pathPending || !agent.hasPath)
            return float.PositiveInfinity;*/

        float rd = agent.remainingDistance;
        if (IsValidRemainingDistance(rd))
            return rd;

      
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

    private static bool IsValidRemainingDistance(float d)
        => !(float.IsInfinity(d) || float.IsNaN(d)) && d >= 0f;

    public static void RotateTowards(this Transform owner, Transform rotateTowards)
    {
        if (owner == null || owner == null
            || rotateTowards == null || rotateTowards == null) return;

        Transform t = owner;
        Transform target = rotateTowards;
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




    public static float SqrDistanceTo(this Vector3 a, Vector3 b)
    {
        Vector3 d = a - b;
        return d.sqrMagnitude;
    }

    public static bool IsSqrDistanceGreaterThan(this float currentDistSq, float initialDistSq, float multiplier = 1f)
    {
        float m2 = multiplier * multiplier;
        return currentDistSq > initialDistSq * m2;
    }
}
