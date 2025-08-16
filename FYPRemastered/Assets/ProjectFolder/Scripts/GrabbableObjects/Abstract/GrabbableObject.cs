using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using Unity.VisualScripting;
using UnityEngine;

public abstract class GrabbableObject : ComponentEvents
{
    protected GrabbableEventManager _grabbableEventManager;
    public bool IsGrabbed { get; protected set; } = false;
    
    [SerializeField] protected PlayerEventManager _playerEventManager;
    protected HandSide _handSide = HandSide.None;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
       _grabbableEventManager = eventManager as GrabbableEventManager;
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
        _handSide = sdkHand == Handedness.Left ? HandSide.Left : HandSide.Right;

        IsGrabbed = true;
        _playerEventManager.Grab(_handSide, IsGrabbed);
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

        _playerEventManager.ReleaseGrabbable(_handSide, IsGrabbed);
        _handSide = HandSide.None;
   
        OnReleased();
    }

   
    protected abstract void OnGrabbed();
    protected abstract void OnReleased();
}
