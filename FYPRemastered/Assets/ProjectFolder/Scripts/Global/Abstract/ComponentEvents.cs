using UnityEngine;

public class ComponentEvents : MonoBehaviour
{
    protected EventManager _eventManager;

    public virtual void RegisterLocalEvents(EventManager eventManager) { _eventManager = eventManager; }

    public virtual void UnRegisterLocalEvents(EventManager eventManager) { _eventManager = null; }

    protected virtual void RegisterGlobalEvents() { }

    protected virtual void UnRegisterGlobalEvents() { }
}
