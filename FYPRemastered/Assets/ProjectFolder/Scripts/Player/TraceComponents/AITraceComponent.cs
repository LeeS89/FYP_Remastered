using UnityEngine;

public class AITraceComponent : TraceComponent
{


    public bool IsWithinView(Transform from, Vector3 targetPosition, float horizontalThreshold, float verticalThreshold)
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

  


    public int EvaluateViewCone(Vector3 start, Vector3 end, float radius, Vector3 direction, float maxdistance, LayerMask targetMask, Vector3[] hitPoints)
    {
        RaycastHit[] hits = Physics.CapsuleCastAll(start, end, radius, direction.normalized, maxdistance, targetMask);

        int hitCount = 0;

        

        foreach (var hit in hits)
        {
           
            if (hitCount < hitPoints.Length)
            {
                hitPoints[hitCount++] = hit.point;
            }

        }

        return hitCount;
    }

    public bool HasLineOfSight(
       Transform from,
       Vector3 target,
       LayerMask blockingMask,
       LayerMask targetMask
       )
    {


        if (Physics.Linecast(from.position, target, out RaycastHit hit, blockingMask | targetMask))
        {
            if (((1 << hit.collider.gameObject.layer) & targetMask) != 0)
            {
                Debug.DrawLine(from.position, target, Color.green);
                Debug.LogError($"Hit target: {hit.collider.gameObject.name}");

                return true;
            }
        }


        return false;
    }

  
}
