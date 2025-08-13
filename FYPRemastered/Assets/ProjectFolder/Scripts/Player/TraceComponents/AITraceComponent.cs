using UnityEngine;

public class AITraceComponent : TraceComponent
{
    private RaycastHit[] _hitBuffer = new RaycastHit[10];


    public int CheckTargetWithinCombatRange(Vector3 traceLocation, Collider[] hitResults, float sphereRadius = 0.2f, LayerMask traceLayer = default)
    {

        //Vector3 start = location.position - location.forward * (capsuleHeight / 2f);  // Bottom of capsule
        //Vector3 end = location.position + location.forward * (capsuleHeight / 2f);    // Top of capsule


        return Physics.OverlapSphereNonAlloc(traceLocation, sphereRadius, hitResults, traceLayer);
        //hitResults = _overlapResults;
        //return hits;

/*
        for (int i = 0; i < hitResults.Length; i++)
        {
            hitResults[i] = null;
        }
        // Clear the results if no objects were found
        return 0;*/

    }


    public bool IsWithinView(Transform from, Vector3 targetPosition, float horizontalThreshold, float verticalThreshold)
    {
        Vector3 localDir = from.InverseTransformDirection(targetPosition - from.position).normalized;
        
        float horizontalAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
        float verticalAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

        return Mathf.Abs(horizontalAngle) <= horizontalThreshold && Mathf.Abs(verticalAngle) <= verticalThreshold;
        /* Vector3 directionToTarget = (targetPosition - from.position).normalized;

         // Horizontal (XZ-plane)
         Vector3 flatForward = new Vector3(from.forward.x, 0f, from.forward.z).normalized;
         Vector3 flatDirection = new Vector3(directionToTarget.x, 0f, directionToTarget.z).normalized;
         float horizontalAngle = Vector3.Angle(flatForward, flatDirection);

         // Vertical (Y-axis offset relative to flat distance)
         float dx = targetPosition.x - from.position.x;
         float dz = targetPosition.z - from.position.z;
         float yOffset = targetPosition.y - from.position.y;

         float verticalAngle = Mathf.Atan2(yOffset, Mathf.Sqrt(dx * dx + dz * dz)) * Mathf.Rad2Deg;

         return horizontalAngle <= horizontalThreshold && Mathf.Abs(verticalAngle) <= verticalThreshold;*/
    }


   


    public int EvaluateViewCone(Vector3 start, Vector3 end, float radius, Vector3 direction, float maxDistance, LayerMask targetMask, Vector3[] hitPoints)
    {

        int hitCount = Physics.CapsuleCastNonAlloc(start, end, radius, direction, _hitBuffer, maxDistance, targetMask);

        int processedCount = Mathf.Min(hitCount, hitPoints.Length);

        for (int i = 0; i < processedCount; i++)
        {
            hitPoints[i] = _hitBuffer[i].point;
        }

        return processedCount;

/*
        RaycastHit[] hits = Physics.CapsuleCastAll(start, end, radius, direction.normalized, maxDistance, targetMask);


         int hitCount = 0;


        foreach (var hit in hits)
        {

            if (hitCount < hitPoints.Length)
            {
                hitPoints[hitCount++] = hit.point;
            }

        }

        return hitCount;*/
    }

  /*  public bool HasLineOfSight(
       Transform from,
       Vector3 target,
       LayerMask blockingMask,
       LayerMask targetMask,
       bool debug = false
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
            else if(((1 << hit.collider.gameObject.layer) & blockingMask) != 0)
            {
                if (debug)
                {
                    Debug.DrawLine(from.position, target, Color.red, 25f);
                    Debug.LogError($"Hit target blocking mask: {hit.collider.gameObject.name}");
                    return false;
                }
            }
        }


        return false;
    }*/


    public bool HasLineOfSight(
       Transform from,
       Vector3 target,
       LayerMask blockingMask,
       LayerMask targetMask,
       Transform ownerTransform,
       Transform fallbackFrom = null,
       bool debug = false
       )
    {
       // Debug.LogError("LOS CALLED");
        RaycastHit hitInfo;
        bool targetWasHit;

        CheckHit(from, target, out hitInfo, out targetWasHit, blockingMask);

        if (!targetWasHit) { return false; }

        if(fallbackFrom != null)
        {
            if(hitInfo.transform.root == from)
            {
                targetWasHit = false;
                CheckHit(fallbackFrom, target, out hitInfo, out targetWasHit, blockingMask);
                if (!targetWasHit) { return false; }

                if (((1 << hitInfo.collider.gameObject.layer) & targetMask) != 0)
                {
                   
                    return true;
                }
               
            }
        }

        if (((1 << hitInfo.collider.gameObject.layer) & targetMask) != 0)
        {
            //Debug.DrawLine(fallbackFrom.position, target, Color.green);
          
            return true;
        }


        return false;
    }

    private void CheckHit(
       Transform from,
       Vector3 target,
       out RaycastHit hit,
       out bool hitTarget,
       LayerMask blockingMask   
       )
    {


        if (Physics.Linecast(from.position, target, out hit, blockingMask))
        {
            hitTarget = true;
        }
        else
        {
            hitTarget = false;
        }


    }

}
