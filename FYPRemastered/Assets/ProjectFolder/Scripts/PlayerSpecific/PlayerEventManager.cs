using System;
using UnityEngine;

public class PlayerEventManager : EventManager
{
    public event Action<Quaternion> OnPlayerRotate;
    public event Action<Vector3> OnPlayerHeightUpdated;
    public event Action<bool> OnMovementGesturePerformedOrReleased;
    public event Action<HandSide, bool> OnGrab;
    public event Action<HandSide, bool> OnReleaseGrabbable;

    public event Action<Vector3> OnMovementUpdated;


   
    public event Func<HandSide, bool> OnCheckIfHandIsGrabbing;

    public bool CheckIfHandIsGrabbing(HandSide side)
    {
        return OnCheckIfHandIsGrabbing?.Invoke(side) ?? false;
    }

  


    #region Locomotion events
    public void MovementGesturePerformedOrReleased(bool performed)
    {
        OnMovementGesturePerformedOrReleased?.Invoke(performed);
    }

    public void PlayerRotate(Quaternion targetrotation)
    {
        OnPlayerRotate?.Invoke(targetrotation);
    }

    public void PlayerHeightUpdated(Vector3 _cameraLocalPos)
    {
        OnPlayerHeightUpdated?.Invoke(_cameraLocalPos);
    }
    #endregion

    #region Grab events
  /*  public void TryGrab(Transform location, string targetID)
    {
        if (OnTryGrab != null)
        {
            OnTryGrab?.Invoke(location, targetID);
        }
    }*/

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
