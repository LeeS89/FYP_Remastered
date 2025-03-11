using UnityEngine;

public class WeaponTraceComponent : MonoBehaviour
{
    [SerializeField] private Transform _traceStart;
    [SerializeField] private Transform _traceEnd;
    [SerializeField] private float _radius = 1f;
    [SerializeField] private LayerMask _layers;
    /*public AudioSource _audioSource;
    public AudioClip _clip; */

    
    private void OnCollisionEnter(Collision collision)
    {
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
        AudioSource audio = ParticlePool.GetFromPool<AudioSource>(PoolType.AudioSRC, impactPosition, Quaternion.identity);
        audio.Play();
            //_audioSource.PlayOneShot(_clip);
            deflectable.Deflect();
          
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
