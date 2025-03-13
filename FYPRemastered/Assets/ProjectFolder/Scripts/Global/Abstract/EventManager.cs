using UnityEngine;

public abstract class EventManager : MonoBehaviour
{
    protected  PoolManager _poolManager;

    public abstract void BindComponentsToEvents();

    public abstract void UnbindComponentsToEvents();

    public virtual void HitParticlePoolInjection(PoolManager poolManager) { }
}
