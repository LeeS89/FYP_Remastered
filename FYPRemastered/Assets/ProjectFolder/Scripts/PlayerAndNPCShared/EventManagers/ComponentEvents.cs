using System;
using UnityEngine;

public abstract class ComponentEvents : MonoBehaviour
{
    protected EventManager _eventManager;

    public virtual void RegisterLocalEvents(EventManager eventManager) => eventManager.OnDeathStatusUpdated += DeathStatusUpdated;

    public virtual void UnRegisterLocalEvents(EventManager eventManager) => eventManager.OnDeathStatusUpdated -= DeathStatusUpdated;

    protected virtual void RegisterGlobalEvents()
    {
        BaseSceneManager._instance.OnSceneStarted += OnSceneStarted;
        BaseSceneManager._instance.OnSceneComplete += OnSceneComplete;
    }

    protected virtual void UnRegisterGlobalEvents() => BaseSceneManager._instance.OnSceneStarted -= OnSceneStarted; // Switch to Scene aggregator

    protected virtual void OnSceneStarted()
    {
        OwnerIsDead = false;
        PlayerIsDead = false;
    }

    public virtual void InitialzeLocalPools() { }
    

    protected virtual void OnSceneComplete() => BaseSceneManager._instance.OnSceneComplete -= OnSceneComplete; // Switch to Scene aggregator

    protected virtual void OnPlayerDeathStatusUpdated(bool isDead) => PlayerIsDead = isDead; 
    public static bool PlayerIsDead { get; protected set; }

    public bool OwnerIsDead { get; protected set; } = false;
    protected virtual void DeathStatusUpdated(bool isDead) => OwnerIsDead = isDead;

    // protected virtual void OnPlayerDied() { }


    // protected virtual void OnPlayerRespawned() { }
}


public abstract class ComponentInit<TServices, TManager> : MonoBehaviour, IServicable<TServices, TManager>
    where TServices : class
    where TManager : EventManagerBase
{

  //  public bool IsDead { get; protected set; } = false; // Remove
    public ISceneService SceneService { get; private set; }

    public abstract void Init(TServices services, TManager manager);
    
    void IServicable.Init(ISceneServiceProvider provider, EventManagerBase manager)
    {
        if (provider is not TServices s) return;
        if (manager is not TManager m) return;
        
        if(provider.TryGetSceneService(out var service))
        {
            SceneService = service;
            SceneService.OnSceneBegin += SceneBegin;
            SceneService.OnSceneEnd += SceneEnd;
        }
       
        Init(s, m);
    }
    
    public abstract void Unload();
    protected virtual void OnSceneBegin() { }
    protected virtual void OnSceneEnd() { }

    //protected virtual void OnDeath(ITargetable targetable) => _sceneService?.OnTargetableDied(targetable);
  //  protected virtual void OnRespawn(ITargetable targetable) => _sceneService?.OnTargetableRespawned(targetable);

    private void SceneBegin() { SceneService.OnSceneBegin -= SceneBegin; OnSceneBegin(); }
    private void SceneEnd() 
    {
        SceneService.OnSceneEnd -= SceneEnd; 
        OnSceneEnd();
        SceneService = null;
    }
   

}
public abstract class TargetableInit<TServices, TManager> : ComponentInit<TServices, TManager>, IServicable<TServices, TManager>, ITargetable
    where TServices : class
    where TManager : EventManagerBase
{
 
    public bool IsDead { get; protected set; } = false;

    public Vector3 Forward => _rootTransform != null ? _rootTransform.forward : transform.forward;

    [Header("The transform of this game object used for targeting purposes")]
    [SerializeField] protected Transform _rootTransform;
    public Transform Transform => _rootTransform != null ? _rootTransform : transform; // Possibly obsolete

    [Header("Trigger area on the game object that other NPC's use as target area for aiming")]
    [SerializeField] protected Collider _targetCollider;
    public Collider TargetableCollider { get; private set; }

    public bool IsStationary { get; protected set; } = true;

    [Header("Mask of this Gamobeject used for targeting purposes")]
    [SerializeField] protected LayerMask _selfTargetMask;
    public LayerMask LayerMask => _selfTargetMask;


    protected virtual void OnDeath() { IsDead = true; SceneService?.OnTargetableDied(this); }
    protected virtual void OnRespawn() { IsDead = false; SceneService?.OnTargetableRespawned(this); }

    public Vector3 Position() => _rootTransform != null ? _rootTransform.position : transform.position;


    [Obsolete]
    public Quaternion Rotation() => _rootTransform == null ? transform.rotation : _rootTransform.rotation;



    protected override void OnSceneBegin() => SetTargetableCollider();


    private void SetTargetableCollider()
    {
        if (_targetCollider == null)
        {
            if (!TryGetComponent<Collider>(out var coll))
            {
                TargetableCollider = gameObject.AddComponent<BoxCollider>();
            }
            else
                TargetableCollider = coll;
        }
        else
        {
            TargetableCollider = _targetCollider;
        }
    }

}
