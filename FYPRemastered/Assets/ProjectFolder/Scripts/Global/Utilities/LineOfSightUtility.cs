using ArmnomadsGames;
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

        Vector3 center = boxCollider.bounds.center + Vector3.up * boxCollider.bounds.extents.y;

        if (Physics.Linecast(from.position, center, out RaycastHit hit, blockingMask))
        {
            if (hit.collider == target || hit.collider == target.transform.root)
            {
                Debug.DrawLine(from.position, center, Color.green);
                canShoot = angleToTarget <= shootAngle * 0.5f;
                return true;
            }
            else
            {
                Debug.LogError($"Hit object: {hit.collider.gameObject.name}");
                Debug.DrawLine(from.position, hit.point, Color.red);
                canShoot = false;
                return false;
            }
        }

        canShoot = false;
        return false;
    }

    public static bool HasLineOfSight(
        Transform from, 
        Collider[] targets, 
        float viewAngle, 
        float shootAngle, 
        LayerMask blockingMask,
        LayerMask targetMask,
        out bool canShoot
        )
    {

        foreach(var target in targets)
        {
            if(target == null) { continue; }

           /* Vector3 directionToTarget = (target.bounds.center - from.position).normalized;
            //float angleToTarget = Vector3.Angle(from.forward, directionToTarget);


            Vector3 flatForward = new Vector3(from.forward.x, 0, from.forward.z).normalized;
            Vector3 flatDirectionToTarget = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;

            float flatAngle = Vector3.Angle(flatForward, flatDirectionToTarget);*/



            if (!IsWithinAimingAngles(from,target.bounds.center, viewAngle * 0.5f, viewAngle *0.5f))
            {
                //Debug.DrawRay(from.position, directionToTarget * 10f, Color.yellow);
                continue;
            }

            Vector3 centerOfTarget = target.bounds.center;
            Vector3 topOfTarget = target.bounds.center + Vector3.up * target.bounds.extents.y;

            if (Physics.Linecast(from.position, topOfTarget, out RaycastHit hit, blockingMask | targetMask))
            {
                if (((1 << hit.collider.gameObject.layer) & targetMask) != 0)
                {
                    Debug.DrawLine(from.position, centerOfTarget, Color.green);
                    Debug.LogError($"Hit target: {hit.collider.gameObject.name}");
                    canShoot = IsWithinAimingAngles(from, target.bounds.center, shootAngle * 0.5f, shootAngle);
                    return true;
                }
            }
        }



        canShoot = false;
        return false;
    }


    public static bool HasLineOfSight(Vector3 from, Collider[] targets, LayerMask blockingMask, LayerMask targetMask)
    {

        foreach (var target in targets)
        {

            Bounds bounds = target.bounds;
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            Vector3[] testPoints = new Vector3[]
            {
                center,
                center + Vector3.up * extents.y,
                center - Vector3.up * extents.y,
                center + Vector3.right * extents.x,
                center - Vector3.right * extents.x
            };

            foreach (var colPoint in testPoints)
            {
                if (Physics.Linecast(from, colPoint, out RaycastHit hit, blockingMask | targetMask))
                {
                    if (((1 << hit.collider.gameObject.layer) & targetMask) != 0)
                    {
                        Debug.DrawLine(from, colPoint, Color.green, 25f);
                        return true;
                    }
                    else
                    {
                        Debug.DrawLine(from, hit.point, Color.red, 25f);
                        return false;
                    }
                }
            }
        }
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

    public static Vector3 GetClosestPointOnNavMesh(Vector3 position, float maxDistance = 8f)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // If no point is found, fallback to original position (or handle differently)
        return position;
    }

    /// <summary>
    /// Checks if the target is within both the horizontal and vertical aim cones of the source.
    /// Optimized to avoid expensive math like Vector3.Distance.
    /// </summary>
    /// <param name="from">The transform representing the source of the aim (e.g., AI head or weapon).</param>
    /// <param name="targetPosition">World position of the target.</param>
    /// <param name="horizontalThreshold">Half of the allowed horizontal aim angle (degrees).</param>
    /// <param name="verticalThreshold">Half of the allowed vertical aim angle (degrees).</param>
    /// <returns>True if within both horizontal and vertical thresholds.</returns>
    public static bool IsWithinAimingAngles(Transform from, Vector3 targetPosition, float horizontalThreshold, float verticalThreshold)
    {
        Vector3 directionToTarget = (targetPosition - from.position).normalized;

        // Horizontal (XZ-plane)
        Vector3 flatForward = new Vector3(from.forward.x, 0f, from.forward.z).normalized;
        Vector3 flatDirection = new Vector3(directionToTarget.x, 0f, directionToTarget.z).normalized;
        float horizontalAngle = Vector3.Angle(flatForward, flatDirection);

        // Vertical (Y-axis offset relative to flat distance)
        float dx = targetPosition.x - from.position.x;
        float dz = targetPosition.z - from.position.z;
        float yOffset = targetPosition.y - from.position.y;

        float verticalAngle = Mathf.Atan2(yOffset, Mathf.Sqrt(dx * dx + dz * dz)) * Mathf.Rad2Deg;

        return horizontalAngle <= horizontalThreshold && Mathf.Abs(verticalAngle) <= verticalThreshold;
    }
}
