using System;
using System.Collections.Generic;
using UnityEngine;

[Obsolete]
public abstract class ComponentEvents : MonoBehaviour
{
    protected EventManagerObsolete _eventManager;

    public virtual void RegisterLocalEvents(EventManagerObsolete eventManager) => eventManager.OnDeathStatusUpdated += DeathStatusUpdated;

    public virtual void UnRegisterLocalEvents(EventManagerObsolete eventManager) => eventManager.OnDeathStatusUpdated -= DeathStatusUpdated;

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


public abstract class ComponentInit<TServices, TManager> : MonoBehaviour, IServicable<TServices, TManager>, IInstanceIdentifiable
    where TServices : class
    where TManager : EventManagerBase
{

  //  public bool IsDead { get; protected set; } = false; // Remove
    public ISceneService SceneService { get; private set; }

    public int EntityId => GetInstanceID();

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

    protected virtual void Update() { }

    /// <summary>
    /// Used as the concrete class's update loop, as the base class handles ticking for any ITickables registered to it.
    /// </summary>
    

}

public abstract class TargetableInit<TServices, TManager> : ComponentInit<TServices, TManager>, IServicable<TServices, TManager>, ITargetable, ITickableRunner
    where TServices : class
    where TManager : EventManagerBase
{
 
    public bool IsDead { get; protected set; } = false;
    private HashSet<ITickable> _tickables;

    public Vector3 Forward => _rootTransform != null ? _rootTransform.forward : transform.forward;

    [Header("The transform of this game object used for targeting purposes")]
    [SerializeField] protected Transform _rootTransform;
    public Transform Transform => _rootTransform != null ? _rootTransform : transform; // Possibly obsolete

    [Header("Trigger area on the game object that other NPC's use as target area for aiming")]
    [SerializeField] protected Collider _targetCollider;
    public Collider TargetableCollider { get; private set; }

    public abstract bool IsMoving();

    [Header("Mask of this Gamobeject used for targeting purposes")]
    [SerializeField] protected LayerMask _selfTargetMask;
    public LayerMask LayerMask => _selfTargetMask;

   
    protected virtual void OnDeath() { IsDead = true; SceneService?.OnTargetableDied(this); }
    protected virtual void OnRespawn() { IsDead = false; SceneService?.OnTargetableRespawned(this); }

    public Vector3? Position() => _rootTransform != null ? _rootTransform.position : transform.position;


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

    protected sealed override void Update()
    {
        if (IsDead) return;

        if(_tickables is not null)
        {
            var e = _tickables.GetEnumerator();

            while (e.MoveNext())
            {
                var t = e.Current;
                
                if(t is null)
                {
                    _tickables.Remove(t);
                    continue;
                }

                t.Tick(Time.deltaTime);
            }
        }

        Tick();
    }

    protected virtual void Tick() { }

    public void Register(ITickable tickable)
    {
        _tickables ??= new(5);
        _tickables.Add(tickable);
    }

    public void Unregister(ITickable tickable)
    {
        if (_tickables is null) return;
        _tickables.Remove(tickable);
    }
}
