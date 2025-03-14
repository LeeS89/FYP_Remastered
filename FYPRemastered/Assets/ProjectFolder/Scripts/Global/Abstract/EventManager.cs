using UnityEngine;

public abstract class EventManager : MonoBehaviour
{
    protected  PoolManager _poolManager;

    public abstract void BindComponentsToEvents();

    public abstract void UnbindComponentsToEvents();

}
