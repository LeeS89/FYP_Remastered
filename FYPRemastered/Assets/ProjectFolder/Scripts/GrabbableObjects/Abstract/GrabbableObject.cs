using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using System;
using Unity.VisualScripting;
using UnityEngine;

public abstract class GrabbableObject : ComponentEvents
{
    protected GrabbableEventManager _grabbableEventManager;
    public HandGrabInteractable _leftInteractable;
    public HandGrabInteractable _rightInteractable;

    public GameObject _leftHand;


    public bool IsGrabbed { get; protected set; } = false;
    
    [SerializeField] protected PlayerEventManager _playerEventManager;
    //protected HandSide _handSide = HandSide.None;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
       _grabbableEventManager = eventManager as GrabbableEventManager;

        foreach (var interactable in GetComponentsInChildren<HandGrabInteractable>())
        {
            
            var handedness = interactable.HandGrabPoses[0];
           
            if (handedness.HandPose.Handedness == Handedness.Left)
            {
                _leftInteractable = interactable;
            }
            else
            {
                _rightInteractable = interactable;
            }
        }

        /*_leftInteractable.enabled = false;
        _rightInteractable.enabled = false;*/

    }

    [Obsolete]
    public HandGrabInteractable Testgrab(HandSide side/*, HandGrabInteractor interactor*/)
    {
       
        
        if (IsGrabbed) { return null; }
        IsGrabbed = true;
        HandGrabInteractable interactable;

        interactable = side == HandSide.Left ? _leftInteractable : _rightInteractable;
        interactable.enabled = true;

        return interactable;

        /*if (side == HandSide.Left)
        {
            interactable = _leftInteractable;
            //_leftHand.SetActive(true);
            _leftInteractable.enabled = true;
            return _leftInteractable;
           // interactor.ForceSelect(_leftInteractable);
        }
        _rightInteractable.enabled = true;
        return _rightInteractable;*/
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _grabbableEventManager = null;
    }

    protected override void RegisterGlobalEvents()
    {
        GameManager.OnPlayerDeathStatusChanged += OnPlayerDeathStatusUpdated;
    }

    protected override void UnRegisterGlobalEvents()
    {
        GameManager.OnPlayerDeathStatusChanged -= OnPlayerDeathStatusUpdated;
    }


    public virtual void Grab(HandGrabPose hgp/*int handSideInt*/)
    {
        if(IsGrabbed || _playerEventManager == null) { return; }

        var sdkHand = hgp.HandPose?.Handedness;
       // _handSide = sdkHand == Handedness.Left ? HandSide.Left : HandSide.Right;

        IsGrabbed = true;
       // _playerEventManager.Grab(_handSide, IsGrabbed);
        OnGrabbed();

        /*_handSide = (HandSide)handSideInt;
        IsGrabbed = true;
        _playerEventManager.Grab(_handSide, IsGrabbed);
        
        OnGrabbed();*/

    }

    

    public virtual void Release(int handSideInt)
    {
        if(!IsGrabbed || _playerEventManager == null) return;
        IsGrabbed = false;

       // _playerEventManager.ReleaseGrabbable(_handSide, IsGrabbed);
       // _handSide = HandSide.None;
   
        OnReleased();
    }

   
    public abstract void OnGrabbed();
    protected abstract void OnReleased();
}
