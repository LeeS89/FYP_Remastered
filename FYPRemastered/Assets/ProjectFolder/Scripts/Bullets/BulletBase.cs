using System.Collections;
using UnityEngine;

public enum BulletType
{
    Normal,
    Fire
}


public abstract class BulletBase : MonoBehaviour, IBulletEvents, IPoolable
{
    [SerializeField] protected GameObject _cachedRoot;
    protected BulletEventManager _eventManager;
    protected IDeflectable _deflectableComponent;
    [SerializeField] protected PoolManager _poolManager;
    [SerializeField] protected BulletType _bulletType;
    [SerializeField] protected GameObject _owner;
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

    public void RegisterEvents(BulletEventManager eventManager)
    {
        if (eventManager == null) { return; }
       
        RegisterDependancies();
        _eventManager = eventManager;
        _eventManager.OnExpired += OnExpired;
        //StartCoroutine(FireDelay());

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
        //_cachedRoot.transform.position = position;
        //_cachedRoot.transform.rotation = rotation;
        if(_deflectableComponent != null)
        {
            _deflectableComponent.ParentOwner = _owner;
        }
        _eventManager.ParticlePlay(this, _bulletType);
        _eventManager.Fired();
    }
    
  
    protected abstract void OnExpired();

    public abstract void Freeze();
    public abstract void UnFreeze();

    public void SetParentPool(PoolManager manager)
    {
       _poolManager = manager;
    }
}
