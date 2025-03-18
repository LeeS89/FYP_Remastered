using System;
using System.Linq;
using UnityEngine;

public class TraceComponent
{
   
    private Collider[] _overlapResults;



    public TraceComponent(int maxSize)
    {
        this._overlapResults = new Collider[maxSize];
    }

    public int CheckForFreezeable(Transform traceLocation, out Collider[] hitResults, float sphereRadius = 0.2f, LayerMask traceLayer = default, bool debug = false)
    {
       
        //Vector3 start = location.position - location.forward * (capsuleHeight / 2f);  // Bottom of capsule
        //Vector3 end = location.position + location.forward * (capsuleHeight / 2f);    // Top of capsule


        bool foundObject = Physics.CheckSphere(traceLocation.position, sphereRadius, traceLayer);

        Color debugColor = foundObject ? Color.green : Color.red; // Green if detected, red if not

        if (debug)
        {

            DebugExtension.DebugWireSphere(traceLocation.position, debugColor, sphereRadius, 1.0f);

        }

        if (foundObject)
        {

            int hits = Physics.OverlapSphereNonAlloc(traceLocation.position, sphereRadius, _overlapResults, traceLayer);
            hitResults = _overlapResults;
            return hits;
            
        }

        hitResults = Array.Empty<Collider>();
        return 0;

    }


}
