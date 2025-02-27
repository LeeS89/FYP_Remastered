using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEventManager : MonoBehaviour
{
    public event Action<Quaternion> OnPlayerRotate;
    public event Action<Vector3> OnPlayerHeightUpdated;
    public event Action<bool> OnPlayerMove;
    public event Action<Transform, MovementGestureController> OnTryGrab;

    private List<IBindableToPlayerEvents> _cachedListeners;

    private void Awake()
    {
        _cachedListeners = new List<IBindableToPlayerEvents>();
        BindComponentsToEvents();
    }


    public void PlayerMove(bool move)
    {
        if(OnPlayerMove != null)
        {
            OnPlayerMove?.Invoke(move);
        }
    }

    public void PlayerRotate(Quaternion targetrotation)
    {
        if (OnPlayerRotate != null)
        {
            OnPlayerRotate?.Invoke(targetrotation);
        }
    }

    public void PlayerHeightUpdated(Vector3 _cameraLocalPos)
    {
        if(OnPlayerHeightUpdated != null)
        {
            OnPlayerHeightUpdated?.Invoke(_cameraLocalPos);
        }
    }

    public void TryGrab(Transform location, MovementGestureController grabbingHand)
    {
        if (OnTryGrab != null)
        {
            OnTryGrab?.Invoke(location, grabbingHand);
        }
    }

    public void BindComponentsToEvents()
    {
        var parentListeners = GetComponents<IBindableToPlayerEvents>();
        _cachedListeners.AddRange(parentListeners);

        var childListeners = GetComponentsInChildren<IBindableToPlayerEvents>(true);
        _cachedListeners.AddRange(childListeners);

        foreach (var listener in _cachedListeners)
        {
            listener.OnBindToPlayerEvents(this);
        }
    }

}
