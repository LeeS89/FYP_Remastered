using UnityEngine;

public abstract class ComponentEvents : MonoBehaviour
{
    protected EventManager _eventManager;

    public virtual void RegisterLocalEvents(EventManager eventManager) { _eventManager = eventManager; }

    public virtual void UnRegisterLocalEvents(EventManager eventManager) { _eventManager = null; }

    protected virtual void RegisterGlobalEvents() { }

    protected virtual void UnRegisterGlobalEvents() { }

    protected virtual void OnSceneStarted() { }

    protected virtual void OnSceneComplete() { }

    protected virtual void OnPlayerDeathStatusUpdated(bool isDead) => PlayerIsDead = isDead;
    public bool PlayerIsDead { get; protected set; }

    // protected virtual void OnPlayerDied() { }


    // protected virtual void OnPlayerRespawned() { }
}
