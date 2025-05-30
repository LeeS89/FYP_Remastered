using UnityEngine;
using UnityEngine.AI;

public static class LineOfSightUtility
{
    //private static readonly NavMeshPath _cachedPath = new NavMeshPath();

    /*public static bool HasLineOfSight(Transform from, Transform target, float viewAngle, LayerMask blockingMask)
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
                return false;            
            }
        }

        return false;
    }*/

    public static bool HasLineOfSight(Transform from, Transform target, float viewAngle, LayerMask blockingMask)
    {
        Vector3 directionToTarget = (target.position - from.position).normalized;

        // First check the field of view angle
        if (Vector3.Angle(from.forward, directionToTarget) > viewAngle * 0.5f)
        {
            Debug.DrawRay(from.position, directionToTarget * 100f, Color.yellow);
            return false;
        }

        // Now use Linecast to check if anything is blocking the view
        if (Physics.Linecast(from.position, (target.position + new Vector3(0,0.1f,0)), out RaycastHit hit, blockingMask))
        {
            if (hit.transform == target)
            {
                Debug.DrawLine(from.position, target.position, Color.green);
                return true;
            }
            else
            {
                Debug.DrawLine(from.position, hit.point, Color.red);
                return false;
            }
        }

        return false;
    }

    public static bool HasLineOfSight(Transform from, Collider target, float viewAngle, LayerMask blockingMask)
    {
        BoxCollider boxCollider = target as BoxCollider;
        Vector3 directionToTarget = (target.transform.position - from.position).normalized;

        // First check the field of view angle
        if (Vector3.Angle(from.forward, directionToTarget) > viewAngle * 0.5f)
        {
            Debug.DrawRay(from.position, directionToTarget * 100f, Color.yellow);
            return false;
        }

        Vector3 top = boxCollider.bounds.center;// + Vector3.up * (boxCollider.bounds.extents.y - 0.05f);
        // Now use Linecast to check if anything is blocking the view
        if (Physics.Linecast(from.position, top, out RaycastHit hit, blockingMask))
        {
            if (hit.collider == target)
            {
                Debug.DrawLine(from.position, top, Color.green);
                return true;
            }
            else
            {
                Debug.DrawLine(from.position, hit.point, Color.red);
                return false;
            }
        }

        return false;
    }

    public static bool HasLineOfSight(Transform from, Collider target, float viewAngle, float shootAngle, LayerMask blockingMask, out bool canShoot)
    {
        BoxCollider boxCollider = target as BoxCollider;
        Vector3 directionToTarget = (target.transform.position - from.position).normalized;

        float angleToTarget = Vector3.Angle(from.forward, directionToTarget);

        // Determine general visibility
        if (angleToTarget > viewAngle * 0.5f)
        {
            Debug.DrawRay(from.position, directionToTarget * 100f, Color.yellow);
            canShoot = false;
            return false;
        }

        Vector3 top = boxCollider.bounds.center;

        if (Physics.Linecast(from.position, top, out RaycastHit hit, blockingMask))
        {
            if (hit.collider == target)
            {
                Debug.DrawLine(from.position, top, Color.green);
                canShoot = angleToTarget <= shootAngle * 0.5f;
                return true;
            }
            else
            {
                Debug.DrawLine(from.position, hit.point, Color.red);
                canShoot = false;
                return false;
            }
        }

        canShoot = false;
        return false;
    }

    public static float GetNavMeshPathDistance(Vector3 from, Vector3 to, NavMeshPath path)
    {
        //NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
        {
            float distance = 0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return distance;
        }
        return float.PositiveInfinity; // No valid path
    }

    public static Vector3 GetClosestPointOnNavMesh(Vector3 position, float maxDistance = 4f)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // If no point is found, fallback to original position (or handle differently)
        return position;
    }
}
