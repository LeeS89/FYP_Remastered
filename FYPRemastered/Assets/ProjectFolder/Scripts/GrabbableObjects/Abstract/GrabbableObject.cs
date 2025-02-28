using UnityEngine;

public abstract class GrabbableObject : MonoBehaviour
{
    protected bool _isGrabbed = false;
    protected MovementGestureController _grabbingHand;
    protected string _grabbingHandID;
    [SerializeField] protected PlayerEventManager _playerEventManager;
   
  
    
    public virtual void Grab(string handID)
    {
        if(_isGrabbed || _playerEventManager == null) { return; }

       
        _grabbingHandID = handID;
        _playerEventManager.Grab(handID);
        _isGrabbed = true;
        OnGrabbed();
      
    }

    

    public virtual void Release()
    {
        if(!_isGrabbed || _playerEventManager == null) return;

        _playerEventManager.ReleaseGrabbable(_grabbingHandID);
       
        _isGrabbed = false;
        _grabbingHandID = "";
        OnReleased();
    }

   
    protected abstract void OnGrabbed();
    protected abstract void OnReleased();
}
