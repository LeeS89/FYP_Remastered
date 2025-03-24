using UnityEngine;

public static class LineOfSightUtility
{
    public static bool HasLineOfSight(Transform from, Transform target, float viewAngle, LayerMask blockingMask)
    {
        Vector3 directionToTarget = (target.position - from.position).normalized;

        if (Vector3.Angle(from.forward, directionToTarget) > viewAngle * 0.5f)
        {
            Debug.DrawRay(from.position, directionToTarget * 100f, Color.yellow);
            return false; 
        }

        if (Physics.Raycast(from.position, directionToTarget, out RaycastHit hit, 100f, blockingMask))
        {
            if (hit.transform == target)
            {
                Debug.DrawRay(from.position, directionToTarget * hit.distance, Color.green);
                return true;
            }
            else
            {
                Debug.DrawRay(from.position, directionToTarget * hit.distance, Color.red);
            }
        }

        return false;
    }
}
