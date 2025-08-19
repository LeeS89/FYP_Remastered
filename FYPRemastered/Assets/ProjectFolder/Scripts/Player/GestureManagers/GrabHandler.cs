using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;
using System;

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
            throw new ArgumentException("GrabHandler requires at least two HandGrabInteractor.");
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
            throw new ArgumentException("GrabHandler requires both Left and Right HandGrabInteractor.");
#else
            Debug.LogError("GrabHandler missing left or right interactor — grabbing will be disabled.");

#endif
        }

        _eventManager.OnCheckIfHandIsGrabbing += IsGrabbing;
    }

    private bool IsGrabbing(HandSide side)
    {
        HandGrabInteractor interactor = side == HandSide.Left ? LeftInteractor : RightInteractor;
        // return interactor != null && interactor.IsGrabbing;
        return interactor != null && interactor.HasSelectedInteractable;
    }


    public void OnInstanceDestroyed()
    {
        _eventManager.OnCheckIfHandIsGrabbing -= IsGrabbing;
        LeftInteractor = null;
        RightInteractor = null;
        _eventManager = null;
    }

}
