using UnityEngine;
using static UnityEngine.ParticleSystem;

public class WeaponCollisionComponent : EventManager, IImpactAudio
{
    [SerializeField] private Transform _traceStart;
    [SerializeField] private Transform _traceEnd;
    [SerializeField] private float _radius = 1f;
    [SerializeField] private LayerMask _layers;
    private PoolManager _audioPoolManager;

    public ParticleSystem _sparks;


    public override void BindComponentsToEvents()
    {
        InterfaceRegistry.Add<IImpactAudio>(this);
    }

    public void SetDeflectAudioPool(PoolManager manager)
    {
        _audioPoolManager = manager;
    }

    public override void UnbindComponentsToEvents()
    {
        InterfaceRegistry.Remove<IImpactAudio>(this);
    }

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

        IDeflectable deflectable = null;
        if (!collision.gameObject.TryGetComponent<IDeflectable>(out deflectable))
        {
            deflectable = collision.gameObject.GetComponentInParent<IDeflectable>() ??
                          collision.gameObject.GetComponentInChildren<IDeflectable>();
        }
       
        if (deflectable == null)
        {
            Debug.LogError("Deflectable is null");
            return;
        }
        ContactPoint contact = collision.contacts[0];
        Vector3 impactPosition = contact.point;
        //AudioPoolExtensions.GetAndPlay(_audioPoolManager, impactPosition, transform.rotation);

        //AudioSource audio = _audioPoolManager.GetAudioSource(impactPosition, transform.rotation);//ParticlePool.GetFromPool<AudioSource>(PoolType.AudioSRC, impactPosition, Quaternion.identity);
        //audio.Play();
        //_audioSource.PlayOneShot(_clip);
        deflectable.Deflect();
          
    }

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
