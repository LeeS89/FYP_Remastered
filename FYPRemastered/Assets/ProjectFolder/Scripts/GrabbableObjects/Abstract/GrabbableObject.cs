using Oculus.Interaction.HandGrab;
using UnityEngine;

public abstract class GrabbableObject : MonoBehaviour
{
    [SerializeField] protected HandGrabInteractable _interactableLeft;
    [SerializeField] protected HandGrabInteractable _interactableRight;
    protected bool _isGrabbed = false;
    protected MovementGestureController _grabbingHand;

    /*protected void Awake()
    {
        _interactableLeft.enabled = false;
        _interactableRight.enabled = false;
    }*/

    public virtual void Grab(MovementGestureController grabbingHand)
    {
        if(_isGrabbed) return;

        if(grabbingHand != null && !grabbingHand.IsGrabbing)
        {
            _grabbingHand = grabbingHand;
            if(grabbingHand._currentHand == MovementGestureController.HandInControl.Left)
            {
                _interactableLeft.enabled = true;
                _interactableRight.enabled = false;
            }
            else
            {
                _interactableRight.enabled = true;
                _interactableLeft.enabled = false;
            }
            grabbingHand.IsGrabbing = true;
            //OnGrabbed();
        }
    }

    public virtual void Release()
    {
        if(_grabbingHand == null) return;

        if (_grabbingHand._currentHand == MovementGestureController.HandInControl.Left)
        {       
            _interactableRight.enabled = true;
        }
        else
        {
            _interactableLeft.enabled = true;
        }
        _isGrabbed = false;
        _grabbingHand.IsGrabbing = false;
        _grabbingHand = null;
    }

    protected abstract void OnGrabbed();
    protected abstract void OnReleased();
}
