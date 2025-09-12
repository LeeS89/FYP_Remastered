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

    protected virtual void UnRegisterGlobalEvents() => BaseSceneManager._instance.OnSceneStarted -= OnSceneStarted;

    protected virtual void OnSceneStarted()
    {
        OwnerIsDead = false;
        PlayerIsDead = false;
    }

    public virtual void InitialzeLocalPools() { }
    

    protected virtual void OnSceneComplete() => BaseSceneManager._instance.OnSceneComplete -= OnSceneComplete;

    protected virtual void OnPlayerDeathStatusUpdated(bool isDead) => PlayerIsDead = isDead;
    public bool PlayerIsDead { get; protected set; }

    public bool OwnerIsDead { get; protected set; } = false;
    protected virtual void DeathStatusUpdated(bool isDead) => OwnerIsDead = isDead;

    // protected virtual void OnPlayerDied() { }


    // protected virtual void OnPlayerRespawned() { }
}
