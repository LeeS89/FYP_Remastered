using Oculus.Interaction;
using System;
using System.Collections;
using UnityEngine;

[Obsolete]
public class Locomotion : ComponentEvents//, IPlayerEvents
{
    [Header("Character Controller Collider values")]
    CharacterController _controller;
    [SerializeField] private float _bodyHeightMin = 0.5f;
    [SerializeField] private float _bodyHeightMax = 2f;
    

    [Header("Movement Speed")]
    [SerializeField] private float _moveSpeed = 4.0f;
    private bool _shouldMoveForward = false;
    private const float GRAVITY = -9.8f;
   // private Vector3 velocity = Vector3.zero;
    private Vector3 _moveDirection = Vector3.zero;
    private float _effectiveMoveSpeed = 0f;
    private float newVelocityY = 0f;
    private Vector3 _newControllerCenter = Vector3.zero;

    private PlayerEventManager _playerEventManager;
    

    //public UniformZoneGridManager _gridManager;

    public bool InputEnabled { get; private set; } = false;
    
   

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
        _playerEventManager = eventManager as PlayerEventManager;
        _playerEventManager.OnKnockbackTriggered += ApplyKnockback;
        _playerEventManager.OnPlayerRotate += HandleRotation;
        _playerEventManager.OnPlayerHeightUpdated += AdjustPlayerHeight;
        _playerEventManager.OnMovementGesturePerformedOrReleased += SetShouldMoveforward;
        
        RegisterGlobalEvents();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        if (eventManager == null)
        {
            Debug.LogError("Player event manager is null");
            return;
        }
        _playerEventManager.OnPlayerRotate -= HandleRotation;
        _playerEventManager.OnPlayerHeightUpdated -= AdjustPlayerHeight;
        _playerEventManager.OnMovementGesturePerformedOrReleased -= SetShouldMoveforward;
        _playerEventManager.OnKnockbackTriggered -= ApplyKnockback;
        UnRegisterGlobalEvents();
        base.UnRegisterLocalEvents(eventManager);
        _playerEventManager = null;


    }

    protected override void RegisterGlobalEvents()
    {
        BaseSceneManager._instance.OnSceneStarted += OnSceneStarted;
        BaseSceneManager._instance.OnSceneComplete += OnSceneComplete;
        GameManager.OnPlayerDeathStatusChanged += OnPlayerDeathStatusUpdated;
        
    }

    protected override void UnRegisterGlobalEvents()
    {
        GameManager.OnPlayerDeathStatusChanged -= OnPlayerDeathStatusUpdated;
        
        BaseSceneManager._instance.OnSceneStarted -= OnSceneStarted;
        BaseSceneManager._instance.OnSceneComplete -= OnSceneComplete;
    }

    //public bool _testMove = false;
    private Vector3 _lastPosition;
    public float movementThreshold = 0.01f;

    public bool _testKnockback = false;

    private void Update()
    {
        if (!InputEnabled) { return; }

        ApplyPlayerMovement();

        if (_testKnockback)
        {
            ApplyKnockback(-transform.forward, 7f, 0.3f);
            _testKnockback = false;
        }

#if UNITY_EDITOR
        float movedDistance = Vector3.Distance(transform.position, _lastPosition);

        if (movedDistance > movementThreshold)
        {
            if (!GameManager.Instance.PlayerHasMoved)
            {
                GameManager.Instance.PlayerHasMoved = true;  // Replace with actual method
            }
        }
        else
        {
            if (GameManager.Instance.PlayerHasMoved)
            {
                GameManager.Instance.PlayerHasMoved = false;
                SceneEventAggregator.Instance.RunClosestPointToPlayerJob(); 
                //MoonSceneManager._instance.TestRun();
            }
        }

        _lastPosition = transform.position;
#endif
   //     SceneEventAggregator.Instance.RunClosestPointToPlayerJob();
        /*if (_testMove)
        {
            GameManager.Instance.PlayerHasMoved = true;
        }
        else
        {
            GameManager.Instance.PlayerHasMoved = false;
        }*/
        /*if (_testMove)
        {
            SetShouldMoveforward(true);
        }
        else
        {
            SetShouldMoveforward(false);
        }*/
    }

    private void LateUpdate()
    {
#if !UNITY_EDITOR
        float movedDistance = Vector3.Distance(transform.position, _lastPosition);

        if (movedDistance > movementThreshold)
        {
            if (!GameManager.Instance.PlayerHasMoved)
            {
                GameManager.Instance.PlayerHasMoved = true;  // Replace with actual method
            }
        }
        else
        {
            if (GameManager.Instance.PlayerHasMoved)
            {
                GameManager.Instance.PlayerHasMoved = false;
                SceneEventAggregator.Instance.RunClosestPointToPlayerJob();
                //MoonSceneManager._instance.TestRun();
            }
        }

        _lastPosition = transform.position;
#endif
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

    public bool _testMove = false;
    private void SetShouldMoveforward(bool move)
    {
        //if (_shouldMoveForward != move)
       // {
            _shouldMoveForward = move;
            GameManager.Instance.PlayerHasMoved = _shouldMoveForward;

            if (!_shouldMoveForward)
            {
               
               //MoonSceneManager._instance.TestRun();
            }
       // }
    }



    private void ApplyPlayerMovement(Vector3? overrideVelocity = null)
    {
        if (_controller == null) { return; }

        Vector3 finalVelocity;

        if (overrideVelocity.HasValue)
        {
            finalVelocity = overrideVelocity.Value;
        }
        else
        {
            _effectiveMoveSpeed = _shouldMoveForward ? _moveSpeed : 0f;

            _moveDirection = transform.forward;
            finalVelocity = _moveDirection * _effectiveMoveSpeed;
            finalVelocity.y = ApplyGravity();
        }

        _controller.Move(finalVelocity * Time.deltaTime);

    }

    private void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        StartCoroutine(HandleKnockback(direction, force, duration));
    }

    private IEnumerator HandleKnockback(Vector3 direction, float force, float duration)
    {
        InputEnabled = false;

        float timer = 0f;
        Vector3 knockbackVelocity = direction * force;

        while(timer < duration)
        {
            ApplyPlayerMovement(knockbackVelocity);
            timer += Time.deltaTime;
            yield return null;
        }

        InputEnabled = true;
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

    protected override void OnSceneStarted()
    {
       
        InputEnabled = true;
        _lastPosition = transform.position;
    }

    protected override void OnSceneComplete()
    {
        InputEnabled = false;
        if (_shouldMoveForward)
        {
            _shouldMoveForward = false;
        }
    }

    protected override void OnPlayerDeathStatusUpdated(bool isDead)
    {
        base.OnPlayerDeathStatusUpdated(isDead);

        InputEnabled = PlayerIsDead;
        if (PlayerIsDead)
        {
            if (_shouldMoveForward)
            {
                _shouldMoveForward = false;
            }
        }
       
       
    }

  /*  protected override void OnPlayerRespawned()
    {
        InputEnabled = true;
    }
*/
   
}
