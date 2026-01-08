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
  //  public bool OwnerIsDead { get; protected set; } = false;
    public bool IsDead { get; protected set; } = false;
    private ISceneService _sceneService;

    public abstract void Init(TServices services, TManager manager);
    
    void IServicable.Init(ISceneServiceProvider provider, EventManagerBase manager)
    {
        if (provider is not TServices s) return;
        if (manager is not TManager m) return;
        
        if(provider.TryGetSceneService(out _sceneService))
        {
            _sceneService.OnSceneBegin += SceneBegin;
            _sceneService.OnSceneEnd += SceneEnd;
        }
       
        Init(s, m);
    }
    
    public abstract void Unload();
    protected virtual void OnSceneBegin() { }
    protected virtual void OnSceneEnd() { }

    //protected virtual void OnDeath(ITargetable targetable) => _sceneService?.OnTargetableDied(targetable);
  //  protected virtual void OnRespawn(ITargetable targetable) => _sceneService?.OnTargetableRespawned(targetable);

    private void SceneBegin() { _sceneService.OnSceneBegin -= SceneBegin; OnSceneBegin(); }
    private void SceneEnd() 
    {
        _sceneService.OnSceneEnd -= SceneEnd; 
        OnSceneEnd();
        _sceneService = null;
    }
   

}
public abstract class TargetableInit<TServices, TManager> : ComponentInit<TServices, TManager>, IServicable<TServices, TManager>, ITargetable
    where TServices : class
    where TManager : EventManagerBase
{
  //  public bool OwnerIsDead { get; protected set; } = false;
    public bool IsDead { get; protected set; } = false;

    public Vector3 Forward => throw new System.NotImplementedException();

    public Transform Transform => throw new System.NotImplementedException();

    public Collider TargetableCollider => throw new System.NotImplementedException();

    public bool IsStationary => throw new System.NotImplementedException();

    public LayerMask LayerMask => throw new System.NotImplementedException();

    private ISceneService _sceneService;

   // public abstract void Init(TServices services, TManager manager);
    
   /* void IServicable.Init(ISceneServiceProvider provider, EventManagerBase manager)
    {
        if (provider is not TServices s) return;
        if (manager is not TManager m) return;
        
        if(provider.TryGetSceneService(out _sceneService))
        {
            _sceneService.OnSceneBegin += SceneBegin;
            _sceneService.OnSceneEnd += SceneEnd;
        }
       
        Init(s, m);
    }*/

  //  public abstract void Unload();
    //protected virtual void OnSceneBegin() { }
    //protected virtual void OnSceneEnd() { }

    protected virtual void OnDeath(ITargetable targetable) => _sceneService?.OnTargetableDied(targetable);
    protected virtual void OnRespawn(ITargetable targetable) => _sceneService?.OnTargetableRespawned(targetable);

    public Vector3 Position()
    {
        throw new System.NotImplementedException();
    }

    public Quaternion Rotation()
    {
        throw new System.NotImplementedException();
    }

    // private void SceneBegin() { _sceneService.OnSceneBegin -= SceneBegin; OnSceneBegin(); }
    /*private void SceneEnd() 
    {
        _sceneService.OnSceneEnd -= SceneEnd; 
        OnSceneEnd();
        _sceneService = null;
    }
   */

}
