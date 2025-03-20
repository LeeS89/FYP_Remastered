using System.Collections;
using UnityEngine;

/*public enum BulletType
{
    Normal,
    Fire
}*/


public abstract class BulletBase : ComponentEvents, IPoolable
{
    [Header("Pools")]
    protected PoolManager _objectPoolManager;
    protected PoolManager _hitParticlePoolManager;

    [Header("Status")]
    [SerializeField] protected float _lifespan = 5f;
    protected float _timeOut;

    [Header("Culling")]
    [SerializeField] protected float _distanceToPlayer;
    [SerializeField] private float _cullDistance = 0.5f;
    [SerializeField] private Animator _anim;

    [Header("Components")]
    protected BulletEventManager _bulletEventManager;
    protected GameObject _owner;
    protected GameObject _cachedRoot;
    protected IDeflectable _deflectableComponent;
   
    //[SerializeField] protected BulletType _bulletType;
    [SerializeField] protected CapsuleCollider _capsuleCollider;
    public LayerMask _layerMask;

   /* protected bool _isAlive = false;
    public bool _isFrozen = false;
    protected bool _isCulled = false;*/

    protected byte _state;
    protected const byte IsAlive = 1 << 0;
    public static readonly byte IsFrozen = 1 << 1;
    protected const byte IsCulled = 1 << 2;


    public GameObject Owner
    {
        get => _owner;
        set
        {
            if (value != null) 
            {
                _owner = value;
            }
        }
    }

   /* public bool Alive
    {
        get => _isAlive;
    }
*/
    protected void SetState(byte state) => _state |= state;
    protected void ClearState(byte state) => _state &= (byte)~state;
    public bool HasState(byte state) => (_state & state) != 0;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        if (_eventManager == null) { return; }
        _bulletEventManager = (BulletEventManager)_eventManager;
        RegisterDependancies();
        _bulletEventManager.OnExpired += OnExpired;
       
        _timeOut = _lifespan;
        
        //StartCoroutine(FireDelay());

    }

   

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _bulletEventManager.OnExpired -= OnExpired;
        base.UnRegisterLocalEvents(eventManager);
        _bulletEventManager = null;
        UnRegisterDependencies();
       
    }

    protected void RegisterDependancies()
    {
        _cachedRoot = transform.parent.gameObject;
        _deflectableComponent = _cachedRoot.GetComponentInChildren<IDeflectable>();
        if (_deflectableComponent != null)
        {
            _deflectableComponent.RootComponent = _cachedRoot;
        }
    }

    protected void UnRegisterDependencies()
    {
        _cachedRoot = null;
        _deflectableComponent = null;
    }

    IEnumerator FireDelay()
    {
        yield return new WaitForEndOfFrame();
        //Initializebullet();
    }

    public virtual void Initializebullet()
    {
        
        if(_deflectableComponent != null)
        {
            _deflectableComponent.ParentOwner = _owner;
        }

        //_isAlive = true;
        SetState(IsAlive);
        _bulletEventManager.ParticlePlay(this);
        _bulletEventManager.Fired();
        _timeOut = _lifespan;
        
    }

    protected virtual void Update()
    {
        //if (_isFrozen || !_isAlive) { return; }
        if (HasState(IsFrozen) || !HasState(IsAlive)) { return; }

        if (_timeOut > 0f)
        {
            _timeOut -= Time.deltaTime;
        }
        else
        {
            OnExpired();
        }

        

    }

    protected abstract void OnExpired();

    public abstract void Freeze();
    public abstract void UnFreeze();

    protected abstract void RemoveFromJob(); 

    public virtual void SetDistanceToPlayer(float distance)
    {
        if (!HasState(IsFrozen)) { return; }
        _distanceToPlayer = distance;
        
        if (_distanceToPlayer <= _cullDistance)
        {
            if (!HasState(IsCulled))
            {
                Cull(true);
            }
        }
        else
        {
            if (HasState(IsCulled))
            {
                Cull();
            }
        }
    }

    protected virtual void Cull(bool cull = false)
    {
        if (cull)
        {
            _anim.SetTrigger("minimize");
            _bulletEventManager.ParticleStop(this/*, _bulletType*/);
            SetState(IsCulled);
            //_isCulled = true;
        }
        else
        {
            _anim.SetTrigger("maximize");
            _bulletEventManager.ParticlePlay(this/*, _bulletType*/);
            ClearState(IsCulled);
            //_isCulled = false;
        }
    }

    public void SetParentPool(PoolManager manager)
    {
        _objectPoolManager = manager;

        /*if (!_eventManager.IsAlreadyInitialized)
        {
            _eventManager.BindComponentsToEvents();
        }*/
    }

    


    #region redundant
    /*protected virtual void CheckForOverlap()
    {

        Vector3 center = _capsuleCollider.transform.TransformPoint(_capsuleCollider.center); // Convert to world position
        float worldHeight = _capsuleCollider.height * _capsuleCollider.transform.lossyScale.y;
        Vector3 start = center - _capsuleCollider.transform.up * (worldHeight * 0.8f);
        Vector3 end = center + _capsuleCollider.transform.up * (worldHeight * 0.8f);
        float capsuleRadius = _capsuleCollider.radius * Mathf.Max(_capsuleCollider.transform.lossyScale.x, _capsuleCollider.transform.lossyScale.y, _capsuleCollider.transform.lossyScale.z);
        int hits = Physics.OverlapCapsuleNonAlloc(start, end, capsuleRadius * 1.65f, _overlapResults, _layerMask);
        //DebugExtension.DebugCapsule(start, end, Color.blue, capsuleRadius * 1.4f);
        for (int i = 0; i < hits; ++i)
        {
            if (_overlapResults[i].gameObject == _capsuleCollider.gameObject)
            {
                continue;
            }
            //Cull(true);
            break;
        }

    }*/

    #endregion

}
