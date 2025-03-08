using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class BulletCollisionComponent : MonoBehaviour, IBulletEvents, IDeflectable
{
    private BulletEventManager _eventManager;
    [SerializeField] private float _deflectSpeed;
    public GameObject _parentOwner;
    public GameObject _rootComponent;
    [SerializeField] private LayerMask _ignoreMask;
    private bool _deflectionProcessed = false;
    public CapsuleCollider _collider;
    

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

    public void Deflect()
    {
        _deflectionProcessed = true;
        Vector3 directionTotarget = TargetingUtility.GetDirectionToTarget(_parentOwner, _rootComponent);
        Quaternion newRotation = Quaternion.LookRotation(directionTotarget);
       
        _eventManager.Deflected(directionTotarget, newRotation, _deflectSpeed);
    }

    public void RegisterEvents(BulletEventManager eventManager)
    {
        if(eventManager == null) { return; }
        _eventManager = eventManager;
      
    }


    private void OnCollisionEnter(Collision collision)
    {
        
        if ((_ignoreMask & (1 << collision.gameObject.layer)) != 0)
        {
            GetComponent<Rigidbody>().excludeLayers = _ignoreMask;
            return; 
        }
        //_parentOwner = null;
        _eventManager.Expired();

    }

    public bool HasDeflectionBeenProcessed()
    {
        return _deflectionProcessed;
    }
}
