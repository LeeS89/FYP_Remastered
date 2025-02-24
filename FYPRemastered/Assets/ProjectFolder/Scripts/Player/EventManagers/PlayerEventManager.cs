using System;
using UnityEngine;

public class PlayerEventManager : MonoBehaviour
{
    public event Action<Quaternion> OnPlayerRotate;

    private void Awake()
    {
        BindComponentsToEvents();
    }


    public void PlayerRotate(Quaternion targetrotation)
    {
        if (OnPlayerRotate != null)
        {
            OnPlayerRotate?.Invoke(targetrotation);
        }
    }

    public void BindComponentsToEvents()
    {
        var parentListeners = GetComponents<IBindableToPlayerEvents>();

        foreach (var listener in parentListeners)
        {
            listener.OnBindToPlayerEvents(this);
        }
    }

}
