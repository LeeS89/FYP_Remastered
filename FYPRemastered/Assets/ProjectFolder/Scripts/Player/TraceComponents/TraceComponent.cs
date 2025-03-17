using System.ComponentModel;
using UnityEngine;


public class TraceComponent : MonoBehaviour, IComponentEvents
{
    [Header("Trace shape size")]
    [SerializeField] private float _sphereRadius = 0.2f;

    [Header("Trace Center")]
    [SerializeField] private Transform _traceLocation;

    [Header("Trace Object Layer")]
    [SerializeField] private LayerMask traceLayer;

    [Header("Trace results max size")]
    [SerializeField] private int maxSize = 50;
    private Collider[] _overlapResults;

    [Header("Trace debugging")]
    [SerializeField] private bool _debug = false;
    //private bool _lockStatus = false;
    public Transform _testLocation = null;
    
    private bool _canTrace = false;

    public bool CanTrace
    {
        get => _canTrace;
        set => _canTrace = value;
    }


    public void RegisterEvents(EventManager eventManager)
    {
        _overlapResults = new Collider[maxSize];
    }

    public void UnRegisterEvents(EventManager eventManager)
    {
        
    }

    private void Update()
    {
        if (!_canTrace) { return; }

        CheckForFreezeable();
    }


    public void CheckForFreezeable()
    {
       
        //Vector3 start = location.position - location.forward * (capsuleHeight / 2f);  // Bottom of capsule
        //Vector3 end = location.position + location.forward * (capsuleHeight / 2f);    // Top of capsule


        bool foundObject = Physics.CheckSphere(_traceLocation.position, _sphereRadius, traceLayer);

        Color debugColor = foundObject ? Color.green : Color.red; // Green if detected, red if not

        if (_debug)
        {
 
            DebugExtension.DebugWireSphere(_traceLocation.position, debugColor, _sphereRadius, 1.0f);
            
        }

        if (foundObject)
        {
            
            int hits = Physics.OverlapSphereNonAlloc(_traceLocation.position, _sphereRadius, _overlapResults, traceLayer);

            for(int i = 0;  i < hits; i++)
            {
                GameObject obj = _overlapResults[i].transform.parent.gameObject;

                BulletBase bullet = obj.GetComponentInChildren<BulletBase>();

                if(bullet == null) { continue; }
                bullet.Freeze();
            }

            /*foreach (Collider col in colliders)
            {
                GameObject obj = col.transform.parent.gameObject;

                //Component[] components = obj.GetComponentsInChildren<Component>();
                BulletBase bulletBase = obj.GetComponentInChildren<BulletBase>();

                if(bulletBase != null)
                {
                    bulletBase.Freeze();
                    //Debug.LogError("Found component: " + obj.GetType());
                }
                *//*foreach (var component in components)
                {
                    // Now you can check the type of each component or perform other logic
                    Debug.LogError("Found component: " + component.GetType());
                }*/
                /*if(col.TryGetComponent<GrabbableObject>(out GrabbableObject obj))
                {
                    
                    return;
                }*//*

            }*/
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

