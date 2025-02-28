using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEventManager : MonoBehaviour
{
    public event Action<Quaternion> OnPlayerRotate;
    public event Action<Vector3> OnPlayerHeightUpdated;
    public event Action<bool> OnPlayerMove;
    public event Action<Transform, string> OnTryGrab;
    public event Action<string> OnGrab;
    public event Action<string> OnReleaseGrabbable;

    private List<IBindableToPlayerEvents> _cachedListeners;

    private void Awake()
    {
        _cachedListeners = new List<IBindableToPlayerEvents>();
        BindComponentsToEvents();
    }

    /// <summary>
    /// Finds all Interface components within the player and 
    /// ensures each component binds to their relevant events
    /// </summary>
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

    #region Locomotion events
    public void PlayerMove(bool move)
    {
        if (OnPlayerMove != null)
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
        if (OnPlayerHeightUpdated != null)
        {
            OnPlayerHeightUpdated?.Invoke(_cameraLocalPos);
        }
    }
    #endregion

    #region Grab events
    public void TryGrab(Transform location, string targetID)
    {
        if (OnTryGrab != null)
        {
            OnTryGrab?.Invoke(location, targetID);
        }
    }

    public void Grab(string targetID)
    {
        if (OnGrab != null)
        {
            OnGrab?.Invoke(targetID);
        }
    }

    public void ReleaseGrabbable(string targetID)
    {
        if (OnReleaseGrabbable != null)
        {
            OnReleaseGrabbable?.Invoke(targetID);
        }
    }
    #endregion


   

   


    

}
