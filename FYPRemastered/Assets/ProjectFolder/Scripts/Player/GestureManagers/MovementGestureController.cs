using System;
using UnityEngine;

public class MovementGestureController : BaseGesture
{
    
    private static HandSide _handInControl = HandSide.None;
    public HandSide _side = HandSide.None;
    

    private static bool _leftHandActive = false;
    public static bool _rightHandActive = false;

    private PlayerEventManager _playerEventManager;
    [SerializeField] private bool _isGrabbing = false;
    [SerializeField] private Transform _grabTraceLocation;

   
    private void ResetFields()
    {
        _handInControl = HandSide.None;
        _leftHandActive = false;
        _rightHandActive = false;
        
    }

    private void Start()
    {
        if(_side == HandSide.None)
        {
            Debug.LogWarning("_current hand must be set to left or right");
        }
    }


    public override void RegisterLocalEvents(EventManager eventManager)
    {
        if (eventManager == null) { return; }
        ResetFields();
        _playerEventManager = (PlayerEventManager)eventManager;
        _playerEventManager.OnGrab += ToggleHasGrabbedObject;
        _playerEventManager.OnReleaseGrabbable += ToggleHasGrabbedObject;
        
        RegisterGlobalEvents();
        
    }

  

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        if (eventManager == null) { return; }

        _playerEventManager.OnGrab -= ToggleHasGrabbedObject;
        _playerEventManager.OnReleaseGrabbable -= ToggleHasGrabbedObject;
        UnRegisterGlobalEvents();
        ResetFields();
        _playerEventManager = null;
    }

    protected override void RegisterGlobalEvents()
    {
        BaseSceneManager._instance.OnSceneStarted += OnSceneStarted;
        BaseSceneManager._instance.OnSceneEnded += OnSceneComplete;
        
        GameManager.OnPlayerDeathStatusChanged += OnPlayerDeathStatusUpdated;
    }

    protected override void UnRegisterGlobalEvents()
    {
        
        GameManager.OnPlayerDeathStatusChanged -= OnPlayerDeathStatusUpdated;
        BaseSceneManager._instance.OnSceneStarted -= OnSceneStarted;
        BaseSceneManager._instance.OnSceneEnded -= OnSceneComplete;
    }


    public override void OnGestureRecognized()
    {

        if (CheckIfCanTriggerMovementPoseResponse())
        {
            if (_playerEventManager != null)
            {
                _playerEventManager.PlayerMove(true);
            }
        }
        SetHandActive(_side, true);

    }

    public override void OnGestureReleased()
    {
        if (CheckIfCanTriggerMovementPoseResponse())
        {
            if (_playerEventManager != null)
            {
                _playerEventManager.PlayerMove(false);
            }
        }
        SetHandActive(_side, false);
    }

    

    private bool CheckIfCanTriggerMovementPoseResponse()
    {
        if (!InputEnabled || _isGrabbing) { return false; }

        if (_handInControl == HandSide.None)
        {
            _handInControl = (_side == HandSide.Left) ? HandSide.Left : HandSide.Right;
            return true;
        }
        return (_handInControl == HandSide.Left && _side == HandSide.Left) ||
        (_handInControl == HandSide.Right && _side == HandSide.Right);

    }

    private void ToggleHasGrabbedObject(HandSide uniqueID, bool grabbing)
    {
        if (_side == uniqueID)
        {
            _isGrabbing = grabbing;
        }
    }


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

    private static void ResetHandInControlIfNeeded()
    {
        if(!_leftHandActive &&  !_rightHandActive)
        {
            _handInControl = HandSide.None;
        }
    }

    protected override void OnPlayerDeathStatusUpdated(bool isDead)
    {
        base.OnPlayerDeathStatusUpdated(isDead);

        if (PlayerIsDead)
        {
            ResetStates();
        }  
    }


    protected override void ResetStates()
    {
        if(_handInControl != HandSide.None) { _handInControl = HandSide.None; }
        if (_leftHandActive) {  _leftHandActive = false; }
        if(_rightHandActive) { _rightHandActive = false; }
    }

    protected override void OnSceneStarted()
    {
        InputEnabled = true;
    }

    protected override void OnSceneComplete()
    {
        InputEnabled = false;
    }



  
}
