using Oculus.Interaction.HandGrab;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : ComponentEvents
{
    [Header("Locomotion Params")]
    [SerializeField] private float _moveSpeed = 4.0f;
    [SerializeField] private float _gravity = -9.8f;

    [Header("Rotation Params")]
    [SerializeField] private Transform _camera;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _rotationThreshold = 20f;
    [SerializeField] private float _stopRotationThreshold = 18f;

    [Header("Dynamic Body Height Params")]
    [SerializeField] private float _bodyHeightMin = 0.5f;
    [SerializeField] private float _bodyHeightMax = 2f;

    [Header("Player Controller Components")]
    private CharacterController _controller;
    private PlayerEventManager _playerEventManager;
    private LocomotionHandler _locomotion;
    private RotationHandler _rotationHandler;
    public TraceComponent TraceComp { get; private set; }

    public HandGrabInteractor _leftInteractor;
    public HandGrabInteractor _rightInteractor;

    public bool InputEnabled { get; private set; } = false;




    private bool IsGrabbing(HandSide side)
    {
        HandGrabInteractor interactor = side == HandSide.Left ? _leftInteractor : _rightInteractor;

        return interactor != null && interactor.IsGrabbing;

    }

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _playerEventManager = eventManager as PlayerEventManager;
       
        if (TryGetComponent<CharacterController>(out CharacterController characterController))
        {
            _controller = characterController;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("Character controller not found, please ensure component exists before use");
#endif
        }
  
        _playerEventManager.OnPlayerRotate += HandleRotation;
        _playerEventManager.OnPlayerHeightUpdated += AdjustPlayerHeight;
        _playerEventManager.OnMovementUpdated += ApplyPlayerMovement;
        _playerEventManager.OnCheckIfHandIsGrabbing += IsGrabbing;
        _locomotion = new LocomotionHandler(_playerEventManager, transform, _moveSpeed, _gravity);
        TraceComp = new TraceComponent();

        SetupRotationHandler();

        RegisterGlobalEvents();
    }

    private void SetupRotationHandler()
    {
        var cfg = new RotationHandler.Config(_camera, transform, _rotationSpeed, _rotationThreshold, _stopRotationThreshold);
        _rotationHandler = new RotationHandler(_playerEventManager, cfg);
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _playerEventManager.OnCheckIfHandIsGrabbing -= IsGrabbing;
        _playerEventManager.OnMovementUpdated -= ApplyPlayerMovement;
        _playerEventManager.OnPlayerRotate -= HandleRotation;
        _playerEventManager.OnPlayerHeightUpdated -= AdjustPlayerHeight;
        UnRegisterGlobalEvents();
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


    private void Update()
    {
        /*if (_leftInteractor.IsGrabbing)
        {
            Debug.LogError("Left Interactor is Grabbing");
        }*/

        if (!InputEnabled) { return; }

        _locomotion?.Tick(_controller.isGrounded);

    }

    private void LateUpdate()
    {
        if (!InputEnabled) { return; }

        _locomotion?.LateTick();
        _rotationHandler?.LateTick();
    }

    private void HandleRotation(Quaternion targetRotation)
    {
        _controller.transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
    }

    private void AdjustPlayerHeight(Vector3 _cameraLocalPos)
    {
        if (_controller == null) { return; }
        Vector3 newControllerCenter;

        _controller.height = Mathf.Clamp(_cameraLocalPos.y, _bodyHeightMin, _bodyHeightMax);
        newControllerCenter.x = _cameraLocalPos.x;
        newControllerCenter.y = _controller.height / 2;
        newControllerCenter.z = _cameraLocalPos.z;

        _controller.center = newControllerCenter;

    }

   


    private void ApplyPlayerMovement(Vector3 velocity)
    {
        if (_controller == null || PlayerIsDead) { return; }

        _controller.Move(velocity * Time.deltaTime);

    }



    protected override void OnSceneStarted()
    {
        if (TraceComp != null)
        {
            _playerEventManager?.TraceComponentSent(TraceComp);
        }
        InputEnabled = true;
    }

    protected override void OnSceneComplete()
    {
        InputEnabled = false;
        _locomotion?.OnInstanceDestroyed();
        _locomotion = null;
        _rotationHandler?.OnInstanceDestroyed();
        _rotationHandler = null;

        TraceComp = null;
    }

    protected override void OnPlayerDeathStatusUpdated(bool isDead)
    {
        base.OnPlayerDeathStatusUpdated(isDead);

        InputEnabled = !PlayerIsDead;
       
    }
}
