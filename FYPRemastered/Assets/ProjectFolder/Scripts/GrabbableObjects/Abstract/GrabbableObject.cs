using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using System;
using Unity.VisualScripting;
using UnityEngine;

public abstract class GrabbableObject : ComponentEvents
{
    protected GrabbableEventManager _grabbableEventManager;
   
    public bool IsGrabbed { get; protected set; } = false;
    
    [SerializeField] protected PlayerEventManager _playerEventManager;
   

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
     
        IsGrabbed = true;
        OnGrabbed();
    }

    

    public virtual void Release(int handSideInt)
    {
        if(!IsGrabbed || _playerEventManager == null) return;

        IsGrabbed = false;
        OnReleased();
    }

   
    public abstract void OnGrabbed();
    protected abstract void OnReleased();
}
