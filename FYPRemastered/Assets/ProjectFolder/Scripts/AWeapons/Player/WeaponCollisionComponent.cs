using System;
using UnityEngine;


public class WeaponCollisionComponent : EventManager
{
    [SerializeField] private Transform _traceStart;
    [SerializeField] private Transform _traceEnd;
    [SerializeField] private float _radius = 1f;
    [SerializeField] private LayerMask _layers;
   
    public ParticleSystem _sparks;


    public override void BindComponentsToEvents() { }
   
    public override void UnbindComponentsToEvents() { }
    

    private void OnCollisionEnter(Collision collision)
    {

        /*if (collision.gameObject.tag == "Finish")
        {

            if (!isImpacting)
            {
                // Get the first contact point (where the collision happened)
                ContactPoint contactP = collision.contacts[0];  // Get the first contact point
                impactPoint = contactP.point;  // Store the impact point

                // Set the emission point for sparks to the contact point
                emitParams.position = impactPoint;
                _sparks.Emit(emitParams, 1);  // Emit a spark at the impact point
                isImpacting = true;  // Mark the lightsaber as in contact
            }
        }*/
        if (CheckForDeflectable(collision.gameObject, out IFreezeAndDeflectable deflectable))
        {
            deflectable.Deflect(ProjectileKickType.Deflect);
        }
        
        /*if (collision.gameObject.TryGetComponent<IDeflectable>(out IDeflectable deflectable)) // Use Same registry as IDamageable for getting
        {
            ContactPoint contact = collision.GetContact(0);
            Vector3 impactPosition = contact.point;
            deflectable.Deflect(ProjectileKickType.Deflect);

            *//* deflectable = collision.gameObject.GetComponentInParent<IDeflectable>() ??
                           collision.gameObject.GetComponentInChildren<IDeflectable>();*//*
        }
        else
        {
            Debug.LogError($"No IDeflectable component found on {collision.gameObject.name}");
        }*/
       
    }

    private bool CheckForDeflectable(GameObject gameObject, out IFreezeAndDeflectable deflectable) 
        => ComponentRegistry.TryGet<IFreezeAndDeflectable>(gameObject, out deflectable);


    /*private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Finish")
        {
            if (isImpacting)
            {
                // Continuously update the impact point while the collision persists
                ContactPoint contact = collision.contacts[0];  // Get the first contact point
                impactPoint = contact.point;  // Update the impact point

                // Emit sparks at the current impact point
                emitParams.position = impactPoint;
                _sparks.Emit(emitParams, 1);  // Continuously emit spark particles at the new impact point
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Finish")
        {
            isImpacting = false;  // Reset the impact state when the collision ends
        }
    }*/

    bool isImpacting = false;
    private ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
    private Vector3 impactPoint;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {

            if (!isImpacting)
            {
                // Get the point of contact between the lightsaber and the collider
                impactPoint = other.ClosestPoint(transform.position);  // Calculate impact point

                // Set the impact point in the particle system
                emitParams.position = impactPoint;  // Start the sparks at the impact point
                _sparks.Emit(emitParams, 1);  // Emit a spark at the impact point
                isImpacting = true;  // Mark the lightsaber as in contact
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            if (isImpacting)
            {
                // Continuously update the impact point during the collision
                impactPoint = other.ClosestPoint(transform.position);  // Update impact point

                // Update the position of the sparks to always follow the impact point
                emitParams.position = impactPoint;
                _sparks.Emit(emitParams, 1);  // Continuously emit spark particles at the new impact point
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            isImpacting = false;
        }
    }


    #region Old Code
    /*public void PerformTrace(Vector3 traceStart, Vector3 traceEnd, Vector3 direction)
    {

        *//*Vector3 startPoint = traceStart;
        Vector3 endPoint = traceEnd;
        float radius = _radius;

        Collider[] hits = Physics.OverlapCapsule(startPoint, endPoint, radius, _layers);

        foreach (var hit in hits)
        {
            //if (hit.TryGetComponent<IDeflectable>(out var deflectable))
            //{
                // Get the closest impact point to the saber
                Vector3 impactPoint = hit.ClosestPoint(transform.position);

                // Calculate the impact direction
                Vector3 impactDirection = (impactPoint - transform.position).normalized;
                IDeflectable deflectable = null;
                if (!hit.gameObject.TryGetComponent<IDeflectable>(out deflectable))
                {
                    deflectable = hit.GetComponentInParent<IDeflectable>() ??
                                  hit.GetComponentInChildren<IDeflectable>();
                }

                if (deflectable == null) { continue; }

                if (!deflectable.HasDeflectionBeenProcessed())
                {
                    deflectable.Deflect();
                }
                // Call the interface function with impact data
                //deflectable.Deflect(impactPoint, impactDirection);
            //}
        }
        DebugExtension.DebugCapsule(traceStart, traceEnd, Color.red, _radius);*/

    /*RaycastHit[] hits = Physics.CapsuleCastAll(traceStart, traceEnd, _radius, direction, 0, _layers);

    foreach (RaycastHit hit in hits)
    {
        IDeflectable deflectable = null;
        if (!hit.collider.gameObject.TryGetComponent<IDeflectable>(out deflectable))
        {
            deflectable = hit.collider.GetComponentInParent<IDeflectable>() ??
                          hit.collider.GetComponentInChildren<IDeflectable>();
        }

        if (deflectable == null) { continue; }

        if (!deflectable.HasDeflectionBeenProcessed())
        {
            deflectable.Deflect();
        }

    }

    DebugExtension.DebugCapsule(traceStart, traceEnd, Color.red, _radius);*//*
}*/
    #endregion

}
