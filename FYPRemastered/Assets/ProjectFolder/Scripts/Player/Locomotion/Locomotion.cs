using Oculus.Interaction;
using UnityEngine;

public class Locomotion : ComponentEvents, IPlayerEvents
{
    [Header("Character Controller Collider values")]
    CharacterController _controller;
    [SerializeField] private float _bodyHeightMin = 0.5f;
    [SerializeField] private float _bodyHeightMax = 2f;
    

    [Header("Movement Speed")]
    [SerializeField] private float _moveSpeed = 4.0f;
    private bool _shouldMoveForward = false;
    private const float GRAVITY = -9.8f;
    private Vector3 velocity = Vector3.zero;
    private Vector3 _moveDirection = Vector3.zero;
    private float _effectiveMoveSpeed = 0f;
    private float newVelocityY = 0f;
    private Vector3 _newControllerCenter = Vector3.zero;

    private PlayerEventManager _playerEventManager;
    private bool _inputEnabled = false;
    
    public bool InputEnabled
    {
        get => _inputEnabled;
        private set => _inputEnabled = value;
    }
   

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        if (eventManager == null)
        {
            Debug.LogError("Player event manager is null");
            return;
        }
        if (TryGetComponent<CharacterController>(out CharacterController characterController))
        {
            _controller = characterController;
        }
        else
        {
            Debug.LogWarning("Character controller not found, please ensure component exists before use");
        }
        _playerEventManager = _eventManager as PlayerEventManager;
        _playerEventManager.OnPlayerRotate += HandleRotation;
        _playerEventManager.OnPlayerHeightUpdated += AdjustPlayerHeight;
        _playerEventManager.OnPlayerMove += SetShouldMoveforward;
        RegisterGlobalEvents();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        if (_eventManager == null)
        {
            Debug.LogError("Player event manager is null");
            return;
        }
        _playerEventManager.OnPlayerRotate -= HandleRotation;
        _playerEventManager.OnPlayerHeightUpdated -= AdjustPlayerHeight;
        _playerEventManager.OnPlayerMove -= SetShouldMoveforward;
        UnRegisterGlobalEvents();
        base.UnRegisterLocalEvents(eventManager);
        _playerEventManager = null;


    }

    protected override void RegisterGlobalEvents()
    {
        GameManager.OnPlayerDied += OnPlayerDied;
        GameManager.OnPlayerRespawn += OnPlayerRespawned;
    }

    protected override void UnRegisterGlobalEvents()
    {
        GameManager.OnPlayerDied -= OnPlayerDied;
        GameManager.OnPlayerRespawn -= OnPlayerRespawned;
    }

    private void Update()
    {
        if (!InputEnabled) { return; }

        ApplyPlayerMovement();
    }

    private void HandleRotation(Quaternion targetRotation)
    {
        _controller.transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
    }

    private void AdjustPlayerHeight(Vector3 _cameraLocalPos)
    {
        if (_controller == null) { return; }

        _controller.height = Mathf.Clamp(_cameraLocalPos.y, _bodyHeightMin, _bodyHeightMax);
        _newControllerCenter.x = _cameraLocalPos.x;
        _newControllerCenter.y = _controller.height / 2;
        _newControllerCenter.z = _cameraLocalPos.z;



        _controller.center = _newControllerCenter;

    }

   
    private void SetShouldMoveforward(bool move)
    {
        _shouldMoveForward = move;
    }



    private void ApplyPlayerMovement()
    {
        if (_controller == null) { return; }

        _effectiveMoveSpeed = _shouldMoveForward ? _moveSpeed : 0f;

        _moveDirection = transform.forward;
        velocity = _moveDirection * _effectiveMoveSpeed;
        velocity.y = ApplyGravity();

        _controller.Move(velocity * Time.deltaTime);

    }

    private float ApplyGravity()
    {
        if(_controller.isGrounded)
        {
            newVelocityY = GRAVITY / 2f;
        }
        else
        {
            newVelocityY += GRAVITY * Time.deltaTime;
        }
       
        return newVelocityY;
    }

    public void OnSceneStarted()
    {
        InputEnabled = true;
    }

    public void OnSceneComplete()
    {
        InputEnabled = false;
        if (_shouldMoveForward)
        {
            _shouldMoveForward = false;
        }
    }

    public void OnPlayerDied()
    {
        InputEnabled = false;
        if(_shouldMoveForward)
        {
            _shouldMoveForward = false;
        }
    }

    public void OnPlayerRespawned()
    {
        InputEnabled = true;
    }

   
}
