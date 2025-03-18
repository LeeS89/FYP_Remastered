using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class BulletCollisionComponent : MonoBehaviour, IComponentEvents, IDeflectable
{
    [Header("Deflection Speed")]
    [SerializeField] private float _deflectSpeed;

    [Header("Game Object Collider")]
    [SerializeField] private CapsuleCollider _collider;

    [Header("Layer to Ignore after deflection")]
    [SerializeField] private LayerMask _ignoreMask;

    private BulletEventManager _eventManager; 
    private GameObject _parentOwner;
    private GameObject _rootComponent;
    
   
   
    public GameObject ParentOwner
    {
        get => _parentOwner;
        set
        {
            if (value != null)
            {
                _parentOwner = value;
            }
        }
    }

    public GameObject RootComponent
    {
        get => _rootComponent;
        set
        {
            _rootComponent = value;
        }
    }

    public void RegisterEvents(EventManager eventManager)
    {
        if (eventManager == null) { return; }
        _eventManager = (BulletEventManager)eventManager;

        _eventManager.OnUnFreeze += Deflect;

    }

    public void UnRegisterEvents(EventManager eventManager)
    {
        _eventManager.OnUnFreeze -= Deflect;
        _eventManager = null;
    }


    private void OnDisable()
    {
        _collider.excludeLayers = 0;
    }
    public void Deflect()
    {
        Vector3 directionTotarget = TargetingUtility.GetDirectionToTarget(_parentOwner, _rootComponent, true);
        Quaternion newRotation = Quaternion.LookRotation(directionTotarget);
       
        _eventManager.Deflected(directionTotarget, newRotation, _deflectSpeed);
    }

    


    private void OnCollisionEnter(Collision collision)
    {
       
        ContactPoint contact = collision.contacts[0];
        Vector3 impactPosition = contact.point;
        
        _eventManager.SpawnHitParticle(impactPosition, Quaternion.identity);

        if ((_ignoreMask & (1 << collision.gameObject.layer)) != 0)
        {
            _collider.excludeLayers = _ignoreMask;
            
            return; 
        }
        
        //_parentOwner = null;
        _eventManager.Expired();
        
    }

    
}
