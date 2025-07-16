using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;

public class TraceComponent
{
  
    public bool IsTargetInCloseProximity(Vector3 position, float radius, int layerMask)
    {
        return Physics.CheckSphere(position, radius, layerMask);
    }

    public int CheckTargetProximity(Transform traceLocation, Collider[] hitResults, float sphereRadius = 0.2f, LayerMask traceLayer = default, bool debug = false)
    {
       
        //Vector3 start = location.position - location.forward * (capsuleHeight / 2f);  // Bottom of capsule
        //Vector3 end = location.position + location.forward * (capsuleHeight / 2f);    // Top of capsule


        bool foundObject = Physics.CheckSphere(traceLocation.position, sphereRadius, traceLayer);

        Color debugColor = foundObject ? Color.green : Color.red; // Green if detected, red if not

        if (debug)
        {

            DebugExtension.DebugWireSphere(traceLocation.position, debugColor, sphereRadius);

        }

        if (foundObject)
        {

            return Physics.OverlapSphereNonAlloc(traceLocation.position, sphereRadius, hitResults, traceLayer);
            //hitResults = _overlapResults;
            //return hits;
            
        }

        for (int i = 0; i < hitResults.Length; i++)
        {
            hitResults[i] = null;
        }
        // Clear the results if no objects were found
        return 0;

    }

   

}


