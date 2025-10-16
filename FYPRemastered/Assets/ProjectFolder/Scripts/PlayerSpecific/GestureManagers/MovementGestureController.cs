using Oculus.Interaction.HandGrab;
using System;
using UnityEngine;


public class MovementGestureController : BaseGesture
{
    [SerializeField] private float _grabbableCheckRadius = 0.2f;
    [SerializeField] private Transform _grabbableCheckAnchor;
    [SerializeField] private int _maxGrabbableCheckcolliders = 2;
   

    public GameObject _sphereTest;
    public HandGrabInteractor _interactor;

    // Hand in control is used to determine which hand is currently controlling the movement gesture.
    private static HandSide _handInControl = HandSide.None;
    public HandSide _side = HandSide.None;
    

    private static bool _leftHandActive = false;
    public static bool _rightHandActive = false;

    private PlayerEventManager _playerEventManager;
    [SerializeField] public bool IsGrabbing { get; private set; } = false;
    

    private void ResetFields()
    {
        _handInControl = HandSide.None;
        _leftHandActive = false;
        _rightHandActive = false;
    

    }

    private void Start()
    {
#if UNITY_EDITOR
        if(_side == HandSide.None)
        {
            Debug.LogWarning("_current hand must be set to left or right");
        }
#endif
    }


    public override void RegisterLocalEvents(EventManager eventManager)
    {
        if (eventManager == null) { return; }
        _playerEventManager = eventManager as PlayerEventManager;
        base.RegisterLocalEvents(_playerEventManager);
        ResetFields();
        RegisterGlobalEvents();
        
    }

  

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(_playerEventManager);
        base.UnRegisterGlobalEvents();
        ResetFields();   
    }

   

    public override void OnGestureRecognized()
    {
        IsGrabbing = _playerEventManager.CheckIfHandIsGrabbing(_side);
       
        if (CheckIfCanTriggerMovementPoseResponse())
        {
            if(_playerEventManager == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("PlayerEventManager is null, ensure it is initialized before use.");
#endif
                return;
            }
            _playerEventManager.MovementGesturePerformedOrReleased(true);

        }
        SetHandActive(_side, true);


    }

    public override void OnGestureReleased()
    {
      
        if (CheckIfCanTriggerMovementPoseResponse())
        {
            if (_playerEventManager != null)
            {
                _playerEventManager.MovementGesturePerformedOrReleased(false);
            }
        }
       // if (IsGrabbing) { IsGrabbing = false; }
        SetHandActive(_side, false);
    }

    

    private bool CheckIfCanTriggerMovementPoseResponse()
    {
        if (IsGrabbing) { return false; }

        if (_handInControl == HandSide.None)
        {
            _handInControl = (_side == HandSide.Left) ? HandSide.Left : HandSide.Right;
            return true;
        }
        return (_handInControl == HandSide.Left && _side == HandSide.Left) ||
        (_handInControl == HandSide.Right && _side == HandSide.Right);

    }

 /*   private void ToggleHasGrabbedObject(HandSide uniqueID, bool grabbing)
    {
        if (_side == uniqueID)
        {
            IsGrabbing = grabbing;
        }
    }*/


    private static void SetHandActive(HandSide currentHand, bool isActive)
    {
        if(currentHand == HandSide.Left)
        {
            _leftHandActive = isActive;
        }
        else if(currentHand == HandSide.Right)
        {
            _rightHandActive = isActive;
        }

        if (isActive) { return; }
        ResetHandInControlIfNeeded();
    }

    /// <summary>
    /// Neither hand is performing the movement gesture, so reset the hand in control.
    /// The movement gesture is available to either hand again
    /// </summary>
    private static void ResetHandInControlIfNeeded()
    {
        if (!_leftHandActive && !_rightHandActive)
        {
            _handInControl = HandSide.None;
        }
    }

    protected override void DeathStatusUpdated(bool isDead)
    {
        base.DeathStatusUpdated(isDead);

        if (OwnerIsDead)
        {
            ResetStates();
        }  
    }


    protected override void ResetStates()
    {
        if(_handInControl != HandSide.None) { _handInControl = HandSide.None; }
        if (_leftHandActive) {  _leftHandActive = false; }
        if(_rightHandActive) { _rightHandActive = false; }
        _playerEventManager.MovementGesturePerformedOrReleased(false);
    }

   

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        ResetStates();
        _playerEventManager = null;
    }

}
