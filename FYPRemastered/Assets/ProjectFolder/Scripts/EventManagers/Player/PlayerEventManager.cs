using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEventManager : EventManager
{
    public event Action<Quaternion> OnPlayerRotate;
    public event Action<Vector3> OnPlayerHeightUpdated;
    public event Action<bool> OnMovementGesturePerformedOrReleased;
    public event Action<Transform, string> OnTryGrab;
    public event Action<HandSide, bool> OnGrab;
    public event Action<HandSide, bool> OnReleaseGrabbable;

    public event Action<Vector3> OnMovementUpdated;

    public event Action<TraceComponent> OnTraceComponentReceived;
    
    //public event Action<HandGrabInteractor, bool> OnGrabbedObject; 
    //public static Dictionary<HandGrabInteractor, bool> _lastGrabbingStates = new Dictionary<HandGrabInteractor, bool>();
   // private List<ComponentEvents> _cachedListeners;

    private void Awake()
    {
       /* _cachedListeners = new List<IBindableToPlayerEvents>();
        BindComponentsToEvents();*/
    }

    /// <summary>
    /// Finds all Interface components within the player and 
    /// ensures each component binds to their relevant events
    /// </summary>
   /* public override void BindComponentsToEvents()
    {
        _cachedListeners = new List<ComponentEvents>();
        
        var childListeners = GetComponentsInChildren<ComponentEvents>(true);
        _cachedListeners.AddRange(childListeners);

        foreach (var listener in _cachedListeners)
        {
            listener.RegisterLocalEvents(this);
        }
    }*/

    /*public override void UnbindComponentsToEvents()
    {
        foreach (var listener in _cachedListeners)
        {
            listener.UnRegisterLocalEvents(this);
        }
        _cachedListeners?.Clear();
        _cachedListeners = null;
    }*/

    public void TraceComponentSent(TraceComponent traceComponent)
    {
        OnTraceComponentReceived?.Invoke(traceComponent);
    }

    #region Locomotion events
    public void MovementGesturePerformedOrReleased(bool performed)
    {
        OnMovementGesturePerformedOrReleased?.Invoke(performed);
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

    /* public void GrabbedObject(HandGrabInteractor handInteractor, bool isGrabbing)
     {
         if (_lastGrabbingStates.TryGetValue(handInteractor, out bool lastState))
         {
             if (lastState == isGrabbing) return; // No change, do nothing
         }

         // Store the updated grabbing state
         _lastGrabbingStates[handInteractor] = isGrabbing;


         OnGrabbedObject?.Invoke(handInteractor, isGrabbing);
     }*/

    public void MovementUpdated(Vector3 movement)
    {
        OnMovementUpdated?.Invoke(movement);
    }
    public void Grab(HandSide uniqueID, bool grabbingState)
    {
        OnGrab?.Invoke(uniqueID, grabbingState);
    }

    public void ReleaseGrabbable(HandSide uniqueID, bool grabbingState)
    {
        OnReleaseGrabbable?.Invoke(uniqueID, grabbingState);
    }

    
    #endregion









}
