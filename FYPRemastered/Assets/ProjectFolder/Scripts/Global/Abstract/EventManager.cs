using UnityEngine;

public abstract class EventManager : MonoBehaviour
{
    protected  PoolManager _poolManager;

    public abstract void BindComponentsToEvents();

    public virtual void ParentPoolInjection(PoolManager poolManager) { }
}
