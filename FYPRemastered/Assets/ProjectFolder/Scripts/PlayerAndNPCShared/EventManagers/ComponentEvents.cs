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
    protected ISceneService _sceneService;

    public abstract void Init(TServices services, TManager manager);
    
    void IServicable.Init(ISceneServiceProvider provider, EventManagerBase manager)
    {
        if (provider is not TServices s) return;
        if (manager is not TManager m) return;
        
        provider.TryGetSceneService(out _sceneService);
       // provider.OnSceneBegin += OnSceneBegin;
       // provider.OnSceneEnd += OnSceneEnd;
        Init(s, m);
    }

    public abstract void Unload();
    protected virtual void OnSceneBegin() { }
    protected virtual void OnSceneEnd() { }
   

}
