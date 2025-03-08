using Oculus.Interaction.HandGrab;
using Unity.VisualScripting;
using UnityEngine;

public abstract class GrabbableObject : MonoBehaviour
{
    protected bool _isGrabbed = false;
    
    [SerializeField] protected PlayerEventManager _playerEventManager;
    protected HandSide _handSide = HandSide.None;
 

    public virtual void Grab(int handSideInt)
    {
        if(_isGrabbed || _playerEventManager == null) { return; }

        _handSide = (HandSide)handSideInt;
        _isGrabbed = true;
        _playerEventManager.Grab(_handSide, _isGrabbed);
        
        OnGrabbed();
      
    }

    

    public virtual void Release(int handSideInt)
    {
        if(!_isGrabbed || _playerEventManager == null) return;
        _isGrabbed = false;

        _playerEventManager.ReleaseGrabbable(_handSide, _isGrabbed);
        _handSide = HandSide.None;
   
        OnReleased();
    }

   
    protected abstract void OnGrabbed();
    protected abstract void OnReleased();
}
