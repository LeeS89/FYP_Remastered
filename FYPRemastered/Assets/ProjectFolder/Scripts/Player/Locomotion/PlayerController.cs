using Oculus.Interaction.HandGrab;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerEventManager))]
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
    private GrabHandler _grabHandler;

   

  
    public bool InputEnabled { get; private set; } = false;

    
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _playerEventManager = eventManager as PlayerEventManager;

        base.RegisterLocalEvents(_playerEventManager);

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
       // _playerEventManager.OnDeathStatusUpdated += DeathStatusUpdated;

        _grabHandler = new GrabHandler(_playerEventManager, GetComponentsInChildren<HandGrabInteractor>(false));
        _locomotion = new LocomotionHandler(_playerEventManager, transform, _moveSpeed, _gravity);
       
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
        base.UnRegisterLocalEvents(_playerEventManager);
        //_playerEventManager.OnDeathStatusUpdated -= DeathStatusUpdated;
        _playerEventManager.OnMovementUpdated -= ApplyPlayerMovement;
        _playerEventManager.OnPlayerRotate -= HandleRotation;
        _playerEventManager.OnPlayerHeightUpdated -= AdjustPlayerHeight;
        base.UnRegisterGlobalEvents();
    }


    public bool _testTrim = false;

    private void Update()
    {
        if (_testTrim)
        {
            ComponentRegistry.TrimAll();
            _testTrim = false;
        }

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
        if (_controller == null || OwnerIsDead) { return; }

        _controller.Move(velocity * Time.deltaTime);

    }



    protected override void OnSceneStarted()
    {
        base.OnSceneStarted();
        InputEnabled = true;
    }

    

    protected override void DeathStatusUpdated(bool isDead)
    {
        base.DeathStatusUpdated(isDead);

        InputEnabled = !OwnerIsDead;
       
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        InputEnabled = false;
        _locomotion?.OnInstanceDestroyed();
        _locomotion = null;
        _rotationHandler?.OnInstanceDestroyed();
        _rotationHandler = null;
       
        _grabHandler.OnInstanceDestroyed();
        _grabHandler = null;
        _playerEventManager = null;
    }
}
