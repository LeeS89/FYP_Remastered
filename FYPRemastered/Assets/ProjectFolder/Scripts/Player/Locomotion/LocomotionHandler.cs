using System.Collections;
using UnityEngine;

public class LocomotionHandler
{
    //NEW
    private PlayerEventManager _eventManager;
    private Transform _playerTransform;
    public float Gravity { get; set; } = -9.8f;
    public bool CanMoveForward { get; private set; } = false;
    public float _moveSpeed { get; set; } = 4.0f;

    public bool Knockedback { get; private set; } = false;

    public bool IsGrounded { get; private set; } = true;

    public LocomotionHandler(PlayerEventManager eventManager, Transform playerTransform, float moveSpeed, float gravity = -9.8f)
    {
        _eventManager = eventManager;
        _playerTransform = playerTransform;
        _moveSpeed = moveSpeed;
        Gravity = gravity;
        _eventManager.OnMovementGesturePerformedOrReleased += SetCanMoveforward;
        _eventManager.OnKnockbackTriggered += ApplyKnockback;
    }

    private void CalculateMovementDirection(Vector3? overrideVelocity = null)
    {
        float effectiveMoveSpeed;
        Vector3 finalVelocity;

        if (overrideVelocity.HasValue)
        {
            finalVelocity = overrideVelocity.Value;
        }
        else
        {
            effectiveMoveSpeed = CanMoveForward ? _moveSpeed : 0f;

            Vector3 moveDirection = _playerTransform.forward;
            finalVelocity = moveDirection * effectiveMoveSpeed;
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
        Knockedback = true; 

        float timer = 0f;
        Vector3 knockbackVelocity = direction * force;

        while (timer < duration)
        {
            CalculateMovementDirection(knockbackVelocity);
            timer += Time.deltaTime;
            yield return null;
        }

        Knockedback = false;
    }

    private float ApplyGravity()
    {
        if (IsGrounded)
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
        _eventManager.OnMovementGesturePerformedOrReleased -= SetCanMoveforward;
        _eventManager.OnKnockbackTriggered -= ApplyKnockback;
        _playerTransform = null;
        _eventManager = null;
    }


   
    private float newVelocityY = 0f;
   

    //public bool _testMove = false;
    private Vector3 _lastPosition;
    public float movementThreshold = 0.01f;

    public void Tick(bool isGrounded, bool testKnockback = false)
    {
        IsGrounded = isGrounded;

        if (!Knockedback)
        {
            CalculateMovementDirection();
        }

        if (testKnockback)
        {
            ApplyKnockback(-_playerTransform.forward, 7f, 0.3f);
            testKnockback = false;
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

    public void LateTick()
    {
#if !UNITY_EDITOR
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
