using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MovementGestureController : BaseGesture
{
    [SerializeField] private float _grabbableCheckRadius = 0.2f;
    [SerializeField] private LayerMask _grabbableMask = default;
    [SerializeField] private Transform _grabbableCheckAnchor;
    [SerializeField] private int _maxGrabbableCheckcolliders = 2;
    [SerializeField] private Collider[] _grabbableCheckResults;

    public GameObject _sphereTest;
    public HandGrabInteractor _interactor;

    private static HandSide _handInControl = HandSide.None;
    public HandSide _side = HandSide.None;
    

    private static bool _leftHandActive = false;
    public static bool _rightHandActive = false;

    private PlayerEventManager _playerEventManager;
    [SerializeField] public bool IsGrabbing { get; private set; } = false;
    [SerializeField] private Transform _grabTraceLocation;

    public SelectorUnityEventWrapper _eventWrapper;
    public TraceComponent TraceComp { get; private set; }

    [Obsolete]
    private void TraceComponentReceived(TraceComponent traceComp)
    {
        TraceComp = traceComp;
        if (TraceComp == null) { return; }
        _grabbableCheckResults = new Collider[_maxGrabbableCheckcolliders];
    }

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
        ResetFields();
       // _playerEventManager.OnGrab += ToggleHasGrabbedObject;
       // _playerEventManager.OnReleaseGrabbable += ToggleHasGrabbedObject;
        _playerEventManager.OnTraceComponentReceived += TraceComponentReceived;
        RegisterGlobalEvents();
        
    }

  

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        if (eventManager == null) { return; }

        _playerEventManager.OnTraceComponentReceived -= TraceComponentReceived;
       // _playerEventManager.OnGrab -= ToggleHasGrabbedObject;
       // _playerEventManager.OnReleaseGrabbable -= ToggleHasGrabbedObject;
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


  //  public bool _testtrace = false;

    private void Update()
    {
        /*if (_interactor != null && _interactor.HasSelectedInteractable)
        {
            Debug.LogError("Interactor has interactable: ");
        }*/
       /* if (_testtrace)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = _grabbableCheckAnchor.position;
            sphere.transform.localScale = Vector3.one * (_grabbableCheckRadius * 2f);

            GameObject.Destroy(sphere, 2f);
            int grabbablesDetected = TraceComp.CheckTargetProximity(_grabbableCheckAnchor, _grabbableCheckResults, _grabbableCheckRadius, _grabbableMask);

            for (int i = 0; i < grabbablesDetected; i++)
            {
                if (_grabbableCheckResults[i].TryGetComponent<Lightsaber>(out Lightsaber ls))
                {
                    _currentInteractable = ls.Testgrab(_side*//*, _interactor*//*);
                    _interactor.ForceSelect(_currentInteractable);
                   // _interactable2 = _interactor.Interactable;
                   // Debug.LogError("Interactable Name is: " + _interactable2.name);
                    //ls.OnGrabbed();
                }
            }
            _testtrace = false;
        }*/
    }
    

    public HandGrabInteractable _currentInteractable;
 
  
    public override void OnGestureRecognized()
    {
        IsGrabbing = _playerEventManager.CheckIfHandIsGrabbing(_side);
       
        if (CheckIfCanTriggerMovementPoseResponse())
        {

            _playerEventManager?.MovementGesturePerformedOrReleased(true);

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
        if (IsGrabbing) { _eventWrapper.WhenUnselected.Invoke(); }
        if(_handInControl != HandSide.None) { _handInControl = HandSide.None; }
        if (_leftHandActive) {  _leftHandActive = false; }
        if(_rightHandActive) { _rightHandActive = false; }
        _playerEventManager.MovementGesturePerformedOrReleased(false);
    }

    protected override void OnSceneStarted()
    {
     
    }

    protected override void OnSceneComplete()
    {
        _grabbableCheckResults = null;
    }

}
