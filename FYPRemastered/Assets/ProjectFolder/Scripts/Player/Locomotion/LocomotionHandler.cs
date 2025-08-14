using System.Collections;
using UnityEngine;

public class LocomotionHandler
{
    //NEW
    private PlayerEventManager _eventManager;
    private Transform _playerTransform;
    public float Gravity { get; private set; } = -9.8f;
    public bool CanMoveForward { get; private set; } = false;
    public float _moveSpeed { get; set; } = 4.0f;
    // private float _effectiveMoveSpeed = 0f;


    public LocomotionHandler(PlayerEventManager eventManager, Transform playerTransform, float moveSpeed)
    {
        _eventManager = eventManager;
        _playerTransform = playerTransform;
        _moveSpeed = moveSpeed;
        _playerEventManager.OnMovementGesturePerformedOrReleased += SetCanMoveforward;
        _playerEventManager.OnKnockbackTriggered += ApplyKnockback;
    }

    private void CalculateMovementDirection(Vector3? overrideVelocity = null)
    {
        float _effectiveMoveSpeed;
        Vector3 finalVelocity;

        if (overrideVelocity.HasValue)
        {
            finalVelocity = overrideVelocity.Value;
        }
        else
        {
            _effectiveMoveSpeed = CanMoveForward ? _moveSpeed : 0f;

            _moveDirection = _playerTransform.forward;
            finalVelocity = _moveDirection * _effectiveMoveSpeed;
            finalVelocity.y = ApplyGravity();
        }

        _eventManager.MovementUpdated(finalVelocity);
        

    }




    private void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        CoroutineRunner.Instance.StartCoroutine(HandleKnockback(direction, force, duration));
    }

    private IEnumerator HandleKnockback(Vector3 direction, float force, float duration)
    {
        //InputEnabled = false; => 

        float timer = 0f;
        Vector3 knockbackVelocity = direction * force;

        while (timer < duration)
        {
            CalculateMovementDirection(knockbackVelocity);
            timer += Time.deltaTime;
            yield return null;
        }

        //InputEnabled = true;
    }

    private float ApplyGravity()
    {
        if (_controller.isGrounded)
        {
            newVelocityY = Gravity / 2f;
        }
        else
        {
            newVelocityY += Gravity * Time.deltaTime;
        }

        return newVelocityY;
    }


    private void SetCanMoveforward(bool move)
    {

        CanMoveForward = move;
        GameManager.Instance.PlayerHasMoved = CanMoveForward;

        if (!CanMoveForward)
        {

            //MoonSceneManager._instance.TestRun();
        }
    }

    public void OnInstanceDestroyed()
    {
        _playerEventManager.OnMovementGesturePerformedOrReleased -= SetCanMoveforward;
        _playerEventManager.OnKnockbackTriggered -= ApplyKnockback;
        _playerTransform = null;
        _eventManager = null;
    }




    // END NEW


    [Header("Character Controller Collider values")]
    CharacterController _controller;
    [SerializeField] private float _bodyHeightMin = 0.5f;
    [SerializeField] private float _bodyHeightMax = 2f;


   
   
    
    // private Vector3 velocity = Vector3.zero;
    private Vector3 _moveDirection = Vector3.zero;
   
    private float newVelocityY = 0f;
    private Vector3 _newControllerCenter = Vector3.zero;

    private PlayerEventManager _playerEventManager;


    //public UniformZoneGridManager _gridManager;

   // public bool InputEnabled { get; private set; } = false;






    //public bool _testMove = false;
    private Vector3 _lastPosition;
    public float movementThreshold = 0.01f;

    public bool _testKnockback = false;

    public void Tick()
    {
       // if (!InputEnabled) { return; }

        CalculateMovementDirection();

        if (_testKnockback)
        {
            ApplyKnockback(-_playerTransform.forward, 7f, 0.3f);
            _testKnockback = false;
        }

#if UNITY_EDITOR
        float movedDistance = Vector3.Distance(_playerTransform.position, _lastPosition);

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

        _lastPosition = _playerTransform.position;
#endif
       
    }


}
