using System.Collections;
using UnityEngine;

public enum BulletType
{
    Normal,
    Fire
}


public abstract class BulletBase : MonoBehaviour, IComponentEvents, IPoolable
{
    protected GameObject _cachedRoot;
    [SerializeField] protected BulletEventManager _eventManager;
    protected IDeflectable _deflectableComponent;
    protected PoolManager _objectPoolManager;
    protected PoolManager _hitParticlePoolManager;
    [SerializeField] protected BulletType _bulletType;
    protected GameObject _owner;

    [SerializeField] protected float _lifespan = 5f;
    protected float _timeOut;
    protected bool _isAlive = false;

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

    public void RegisterEvents(EventManager eventManager)
    {
        if (eventManager == null) { return; }
       
        RegisterDependancies();
        _eventManager.OnExpired += OnExpired;
        _timeOut = _lifespan;
        //StartCoroutine(FireDelay());

    }

    public void UnRegisterEvents(EventManager eventManager)
    {
        _eventManager.OnExpired -= OnExpired;
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
        _isAlive = true;
        _eventManager.ParticlePlay(this, _bulletType);
        _eventManager.Fired();
        _timeOut = _lifespan;
    }

    protected void Update()
    {
        if (!_isAlive) { return; }

        if(_timeOut > 0f)
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

    public void SetParentPool(PoolManager manager)
    {
        _objectPoolManager = manager;

        if (!_eventManager.IsAlreadyInitialized)
        {
            _eventManager.BindComponentsToEvents();
        }
    }

    
}
