using UnityEngine;

public interface IComponentEvents
{
    public void RegisterEvents(EventManager eventManager);

    public void UnRegisterEvents(EventManager eventManager);
}
