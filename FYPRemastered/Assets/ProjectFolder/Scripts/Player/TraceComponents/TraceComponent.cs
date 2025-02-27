//using System.Diagnostics;
using UnityEngine;


public class TraceComponent : MonoBehaviour, IBindableToPlayerEvents
{
    public float capsuleRadius = 0.2f;  // Thickness of the capsule
    public float capsuleHeight = 0.3f;  // Height of the capsule
    public LayerMask grabbableLayer;
    public bool _debug = false;

    public Transform _testLocation = null;


    public void OnBindToPlayerEvents(PlayerEventManager eventManager)
    {
        if(eventManager != null)
        {
            eventManager.OnTryGrab += CheckForGrabbable;
        }
    }

    private void Update()
    {
        if(_testLocation != null)
        {
            //TestDebug();
        }
    }


    private void CheckForGrabbable(Transform location, MovementGestureController grabbingHand)
    {
        _testLocation = location;
        Vector3 start = location.position - location.forward * (capsuleHeight / 2f);  // Bottom of capsule
        Vector3 end = location.position + location.forward * (capsuleHeight / 2f);    // Top of capsule


        bool foundObject = Physics.CheckCapsule(start, end, capsuleRadius, grabbableLayer);

        Color debugColor = foundObject ? Color.green : Color.red; // Green if detected, red if not

        if (_debug)
        {
            // ✅ Use DebugExtension to draw a capsule in Gizmos
            DebugExtension.DebugCapsule(start, end, debugColor, capsuleRadius, 1.0f);
        }

        if (foundObject)
        {
            // Check for a GrabbableObject component
            Collider[] colliders = Physics.OverlapCapsule(start, end, capsuleRadius, grabbableLayer);

            foreach (Collider col in colliders)
            {
                GrabbableObject grabbable = col.GetComponent<GrabbableObject>();
                if (grabbable != null)
                {
                    grabbable.Grab(grabbingHand);
                    return;
                    
                }
            }
        }
    }

    /*public void TestDebug()
    {
        Vector3 start = _testLocation.position - _testLocation.up * (capsuleHeight / 2f);  // Bottom of capsule
        Vector3 end = _testLocation.position + _testLocation.up * (capsuleHeight / 2f);

        // Draw the capsule's axis line
        UnityEngine.Debug.DrawLine(start, end, Color.green, 0.1f);

        // Draw capsule thickness by drawing rays outward from the start and end points
        Debug.DrawRay(start, _testLocation.right * capsuleRadius, Color.red, 0.1f);
        Debug.DrawRay(start, -_testLocation.right * capsuleRadius, Color.red, 0.1f);
        Debug.DrawRay(start, _testLocation.forward * capsuleRadius, Color.red, 0.1f);
        Debug.DrawRay(start, -_testLocation.forward * capsuleRadius, Color.red, 0.1f);

        Debug.DrawRay(end, _testLocation.right * capsuleRadius, Color.red, 0.1f);
        Debug.DrawRay(end, -_testLocation.right * capsuleRadius, Color.red, 0.1f);
        Debug.DrawRay(end, _testLocation.forward * capsuleRadius, Color.red, 0.1f);
        Debug.DrawRay(end, -_testLocation.forward * capsuleRadius, Color.red, 0.1f);
    }*/

    /*private void OnDrawGizmos()
    {
        if (!_debug) return; // Only draw when debugging is enabled
        if (_testLocation != null)
        {
            Vector3 start = _testLocation.localPosition - _testLocation.up * (capsuleHeight / 2f);  // Bottom sphere center
            Vector3 end = _testLocation.localPosition + _testLocation.up * (capsuleHeight / 2f);    // Top sphere center

            bool foundObject = Physics.CheckCapsule(start, end, capsuleRadius, grabbableLayer);
            Color debugColor = foundObject ? Color.green : Color.red;  // Green if detected, red if not

            Gizmos.color = debugColor;

            // Draw capsule's end spheres
            Gizmos.DrawWireSphere(start, capsuleRadius);  // Bottom half-sphere
            Gizmos.DrawWireSphere(end, capsuleRadius);    // Top half-sphere

            // Draw capsule's cylindrical body
            Gizmos.DrawLine(start + _testLocation.right * capsuleRadius, end + _testLocation.right * capsuleRadius);
            Gizmos.DrawLine(start - _testLocation.right * capsuleRadius, end - _testLocation.right * capsuleRadius);
            Gizmos.DrawLine(start + _testLocation.forward * capsuleRadius, end + _testLocation.forward * capsuleRadius);
            Gizmos.DrawLine(start - _testLocation.forward * capsuleRadius, end - _testLocation.forward * capsuleRadius);
        }
    }*/

    
}

