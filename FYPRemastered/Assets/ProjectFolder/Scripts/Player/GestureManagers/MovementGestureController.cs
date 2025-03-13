using UnityEngine;

public class MovementGestureController : BaseGesture, IComponentEvents
{
   
    private static HandSide _handInControl = HandSide.None;
    public HandSide _side = HandSide.None;
    

    private static bool _leftHandActive = false;
    public static bool _rightHandActive = false;

    private PlayerEventManager _playerEventManager;
    [SerializeField] private bool _isGrabbing = false;
    [SerializeField] private Transform _grabTraceLocation;

    

    private void Awake()
    {
        InitializeFields();
    }

    private void InitializeFields()
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

    /*public void OnBindToPlayerEvents(PlayerEventManager eventManager)
    {
        if (eventManager == null) { return; }

        _playerEventManager = eventManager;
        _playerEventManager.OnGrab += ToggleHasGrabbedObject;
        _playerEventManager.OnReleaseGrabbable += ToggleHasGrabbedObject;
    }

    public void OnUnBindToPlayerEvents(PlayerEventManager eventManager)
    {
        if (eventManager == null) { return; }

        _playerEventManager.OnGrab -= ToggleHasGrabbedObject;
        _playerEventManager.OnReleaseGrabbable -= ToggleHasGrabbedObject;
        _playerEventManager = null;
    }*/

    public void RegisterEvents(EventManager eventManager)
    {
        if (eventManager == null) { return; }

        _playerEventManager = (PlayerEventManager)eventManager;
        _playerEventManager.OnGrab += ToggleHasGrabbedObject;
        _playerEventManager.OnReleaseGrabbable += ToggleHasGrabbedObject;
    }

    public void UnRegisterEvents(EventManager eventManager)
    {
        if (eventManager == null) { return; }

        _playerEventManager.OnGrab -= ToggleHasGrabbedObject;
        _playerEventManager.OnReleaseGrabbable -= ToggleHasGrabbedObject;
        _playerEventManager = null;
    }

    public override void OnGestureRecognized()
    {
        //_playerEventManager.TryGrab(_grabTraceLocation, _instanceID);

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
        if (_isGrabbing) { return false; }

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

   
}
