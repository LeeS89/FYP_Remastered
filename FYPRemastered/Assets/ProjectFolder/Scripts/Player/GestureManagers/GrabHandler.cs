using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;
using System;
using Oculus.Interaction;

public class GrabHandler
{
    public HandGrabInteractor LeftInteractor { get; set; }
    public HandGrabInteractor RightInteractor { get; set; }
    private PlayerEventManager _eventManager;
    

    public GrabHandler(PlayerEventManager eventManager, HandGrabInteractor[] interactors)
    {
        _eventManager = eventManager;

        if (interactors == null || interactors.Length < 2)
        {
#if UNITY_EDITOR
            throw new ArgumentException("GrabHandler requires at least two HandGrabInteractors.");
#else
            Debug.LogError("GrabHandler initialized with no interactors — grabbing will be disabled.");
            return;
#endif

        }

        foreach (var interactor in interactors)
        {
            if (interactor.Hand.Handedness == Handedness.Left)
            {
                LeftInteractor = interactor;
            }
            else if (interactor.Hand.Handedness == Handedness.Right)
            {
                RightInteractor = interactor;
            }
        }

        if (LeftInteractor == null || RightInteractor == null)
        {
#if UNITY_EDITOR
            throw new ArgumentException("GrabHandler requires both Left and Right HandGrabInteractors.");
#else
            Debug.LogError("GrabHandler missing left or right interactor — grabbing will be disabled.");
            return;
#endif
        }

        _eventManager.OnCheckIfHandIsGrabbing += IsGrabbing;
        _eventManager.OnDeathStatusUpdated += DropGrabbable;
    }

    private bool IsGrabbing(HandSide side)
    {
        HandGrabInteractor interactor = side == HandSide.Left ? LeftInteractor : RightInteractor;
        // return interactor != null && interactor.IsGrabbing;
        return interactor != null && interactor.HasSelectedInteractable;
    }

    public HandGrabInteractable _it;
    public Lightsaber _lightsaber;
    public PointableUnityEventWrapper _eventWrapper;
    public PointerEvent _event;
    private void DropGrabbable(bool ownerDied)
    {
        if (ownerDied)
        {

            if (LeftInteractor != null && LeftInteractor.HasSelectedInteractable)
            {
                /*_it = LeftInteractor.SelectedInteractable;
                _eventWrapper = _it.GetComponent<PointableUnityEventWrapper>();
                // _lightsaber = _it.GetComponentInParent<Lightsaber>();
                if (_eventWrapper != null)
                {
                    //_event = 
                   // _eventWrapper.WhenUnselect.Invoke();
                    //_lightsaber.Release(6);
                }*/
                LeftInteractor.ForceRelease();

            }
            if (RightInteractor != null && RightInteractor.HasSelectedInteractable)
            {
               /* _it = RightInteractor.SelectedInteractable;
                _eventWrapper = _it.GetComponent<PointableUnityEventWrapper>();
                // _lightsaber = _it.GetComponentInParent<Lightsaber>();
                if (_eventWrapper != null)
                {
                   // _lightsaber.Release(6);
                }*/
                RightInteractor.ForceRelease();

            }
        }
        LeftInteractor.enabled = !ownerDied;
        RightInteractor.enabled = !ownerDied;
    }

    public void OnInstanceDestroyed()
    {
        _eventManager.OnCheckIfHandIsGrabbing -= IsGrabbing;
        _eventManager.OnDeathStatusUpdated -= DropGrabbable;
        LeftInteractor = null;
        RightInteractor = null;
        _eventManager = null;
    }

}
