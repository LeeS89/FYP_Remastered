using Meta.WitAi;
using Oculus.Interaction.Input;
using System;
using System.Collections;
using UnityEngine;

public class MovementGestureController : MonoBehaviour, IBindableToPlayerEvents
{
    public enum HandInControl
    {
        None,
        Left,
        Right
    }
    private PlayerEventManager _playerEventManager;

    private static HandInControl _handInControl = HandInControl.None;
    public HandInControl _currentHand = HandInControl.None;
    private static bool _leftHandActive = false;
    public static bool _rightHandActive = false;
    private bool _isGrabbing = false;
    [SerializeField] private Transform _grabTraceLocation;

    private Coroutine _grabCheckDelay;
    private WaitForSeconds _delay;
    public float _delaytime = 0.1f;

    public bool IsGrabbing
    {
        get { return _isGrabbing; }
        set { _isGrabbing = value; }
    }
    
    private void Awake()
    {
        _delay = new WaitForSeconds(_delaytime);
        _handInControl = HandInControl.None;
        _leftHandActive = false;
        _rightHandActive = false;
    }

    private void Start()
    {
        if(_currentHand == HandInControl.None)
        {
            Debug.LogWarning("_current hand must be set to left or right");
        }
    }

    public void OnBindToPlayerEvents(PlayerEventManager eventManager)
    {
        if (eventManager == null) { return; }

        _playerEventManager = eventManager;
    }


    public void MovementPoseRecognized()
    {
        /*if(_grabCheckDelay == null)
        {
            _grabCheckDelay = StartCoroutine(GrabCheck());
        }*/
       
        _playerEventManager.TryGrab(_grabTraceLocation, this);

        if (CheckIfCanTriggerMovementPoseResponse())
        {
            if (_playerEventManager != null)
            {
                _playerEventManager.PlayerMove(true);
            }
        }
        SetHandActive(_currentHand, true);
    }

    private IEnumerator GrabCheck()
    {
        _playerEventManager.TryGrab(_grabTraceLocation, this);
        yield return _delay;

        if (CheckIfCanTriggerMovementPoseResponse())
        {
            if (_playerEventManager != null)
            {
                _playerEventManager.PlayerMove(true);
            }
        }
        _grabCheckDelay = null;
    }

    public void MovementPoseReleased()
    {
        if (CheckIfCanTriggerMovementPoseResponse())
        {
            if(_playerEventManager != null)
            {
                _playerEventManager.PlayerMove(false);
            }
        }
        SetHandActive(_currentHand, false);
    }


    private bool CheckIfCanTriggerMovementPoseResponse()
    {
        if (_isGrabbing) { return false; }

        if(_handInControl == HandInControl.None)
        {
            _handInControl = (_currentHand == HandInControl.Left) ? HandInControl.Left : HandInControl.Right;
            return true;
        }
        return (_handInControl == HandInControl.Left && _currentHand == HandInControl.Left) ||
        (_handInControl == HandInControl.Right && _currentHand == HandInControl.Right);
    
    }

    private static void SetHandActive(HandInControl currentHand, bool isActive)
    {
        if(currentHand == HandInControl.Left)
        {
            _leftHandActive = isActive;
        }
        else if(currentHand == HandInControl.Right)
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
            _handInControl = HandInControl.None;
        }
    }

}
