using Meta.XR.ImmersiveDebugger;
using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;

public class TraceComponent
{
  
    public bool IsTargetWithinRange(Vector3 position, float radius, int layerMask, bool debug = false, float debugDuration = 0f)
    {
        if (debug)
        {

            DebugExtension.DebugWireSphere(position, Color.blue, radius, debugDuration);

        }

       /* if (foundObject)
        {

            return Physics.OverlapSphereNonAlloc(traceLocation.position, sphereRadius, hitResults, traceLayer);
            //hitResults = _overlapResults;
            //return hits;

        }*/
        return Physics.CheckSphere(position, radius, layerMask);
    }

    public int CheckTargetProximity(Transform traceLocation, Collider[] hitResults, float sphereRadius = 0.2f, LayerMask traceLayer = default, bool debug = false)
    {

        //Vector3 start = location.position - location.forward * (capsuleHeight / 2f);  // Bottom of capsule
        //Vector3 end = location.position + location.forward * (capsuleHeight / 2f);    // Top of capsule


        bool foundObject = IsTargetWithinRange(traceLocation.position, sphereRadius, traceLayer);//Physics.CheckSphere(traceLocation.position, sphereRadius, traceLayer);

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

   /* public int CheckTargetWithinCombatRange(Transform traceLocation, Collider[] hitResults, float sphereRadius = 0.2f, LayerMask traceLayer = default)
    {

        //Vector3 start = location.position - location.forward * (capsuleHeight / 2f);  // Bottom of capsule
        //Vector3 end = location.position + location.forward * (capsuleHeight / 2f);    // Top of capsule


        return Physics.OverlapSphereNonAlloc(traceLocation.position, sphereRadius, hitResults, traceLayer);
        //hitResults = _overlapResults;
        //return hits;


        *//*for (int i = 0; i < hitResults.Length; i++)
        {
            hitResults[i] = null;
        }
        // Clear the results if no objects were found
        return 0;
*//*
    }
*/


}




