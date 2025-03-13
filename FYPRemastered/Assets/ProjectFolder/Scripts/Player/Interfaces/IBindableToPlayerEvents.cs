using UnityEngine;

public interface IBindableToPlayerEvents
{
    void OnBindToPlayerEvents(PlayerEventManager eventManager);

    void OnUnBindToPlayerEvents(PlayerEventManager eventManager);
}
