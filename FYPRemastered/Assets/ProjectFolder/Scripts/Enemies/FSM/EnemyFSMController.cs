using System;
using UnityEngine;
using UnityEngine.AI;


public partial class EnemyFSMController : FSMControllerBase
{
    [Header("Animation Components")]
    [SerializeField] private Animator _anim;
    private EnemyAnimController _animController;

    // Depending on wether _currentState is == _startionary or a moving state,
    // this action will either stop the agent immediately or measure the path to the destination
    private Action _destinationCheckAction;
    
    // When true, the agent will aloways face its target
    private bool _rotatingTowardsTarget = false;

    [Header("Agent and animation speed values")]
    private float _targetSpeed = 0f;
    private float _lerpSpeed = 0f;
   // private bool _movementChanged = false;
    private float _previousDirection = 0f;
  
    private AlertStatus _alertStatus = AlertStatus.None;

 
    #region Event Registrations
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
    
        RegisterGlobalEvents();
        SetupFSM();

    }

    #endregion


    #region Animation Updates

   

    protected override void ToggleAgentControlledRotationToTarget(bool rotate)
    {
        if (_rotatingTowardsTarget == rotate) return;
       
        _agent.updateRotation = !rotate;
        _rotatingTowardsTarget = rotate;
    }

    protected override void UpdateAgentSpeedValues(float speed, float lerpSpeed)
    {
        _lerpSpeed = lerpSpeed;
        _targetSpeed = speed;

        //  _movementChanged = true;
    }

    private void UpdateAgentSpeed()
    {

       // if (!_movementChanged) { return; }
        float smoothedSpeed = Mathf.Lerp(_agent.speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        _agent.speed = smoothedSpeed;

        float _currentSpeed = _agent.speed;

        if (Mathf.Approximately(_agent.speed, _targetSpeed))
        {
            _agent.speed = _targetSpeed;
        }
      /*  if (Mathf.Abs(_agent.speed - _targetSpeed) <= 0f)
        {
            _movementChanged = false;
        }*/

        // _animController.UpdateSpeed(smoothedSpeed);

        /* if (Mathf.Abs(_agent.speed - _targetSpeed) <= 0.001f)
         {

             _agent.speed = _targetSpeed;
             //_animController.UpdateSpeed(_targetSpeed);
            // _movementChanged = false;
         }*/

    }

    [SerializeField] private float dampTime = 0.1f;    // How long it takes to reach the target
    private float _speedVelocity;                      // Internal ref for SmoothDamp
    private float _directionVelocity;                  // Internal ref for SmoothDamp
    private float _currentSpeed;                       // Smoothed speed value
    private float _currentDirection;

    private void UpdateAnimator()
    {
        if (_animController == null || _agent == null)
            return;

        // Project velocity onto the XZ plane
        Vector3 velocity = _agent.velocity;
        velocity.y = 0f;

        // Dead-zone: treat micro-movement as zero
        if (velocity.sqrMagnitude < 0.01f)
        {
            // Smoothly return to idle/forward-facing
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, 0f, ref _speedVelocity, dampTime);
            _currentDirection = Mathf.SmoothDamp(_currentDirection, 0f, ref _directionVelocity, dampTime);
            _animController.UpdateBlendTreeParams(_currentSpeed, _currentDirection);
            return;
        }

        // Compute raw speed (magnitude) and raw direction (-1 to +1)
        float rawSpeed = velocity.magnitude;
        Vector3 forward = transform.forward;
        Vector3 dirNorm = velocity.normalized;

        // Invert speed if moving backwards
        if (Vector3.Dot(forward, dirNorm) < 0f)
            rawSpeed *= -1f;

        // Signed angle between facing and movement direction, then normalize
        float angle = Vector3.SignedAngle(forward, dirNorm, Vector3.up);
        float rawDirection = Mathf.Clamp(angle / 90f, -1f, 1f);

        // Smoothly interpolate toward the raw values
        _currentSpeed = Mathf.SmoothDamp(_currentSpeed, rawSpeed, ref _speedVelocity, dampTime);
        _currentDirection = Mathf.SmoothDamp(_currentDirection, rawDirection, ref _directionVelocity, dampTime);

        // Send the smoothed values to your blend tree
        _animController.UpdateBlendTreeParams(_currentSpeed, _currentDirection);

        /*if(_animController == null) { return; }

        Vector3 moveDir = _agent.velocity;
        moveDir.y = 0f;
        moveDir.Normalize();
        Vector3 forward = transform.forward;
        float directionAngle = Vector3.SignedAngle(forward, moveDir, Vector3.up);
        float normalizedDirection = directionAngle / 90f; // Normalize to -1 to 1 range

        float dot = Vector3.Dot(forward, moveDir);
        float speed = _agent.speed;
        if (dot < 0)
        {
            speed *= -1;
        }

        _animController.UpdateBlendTreeParams(speed, normalizedDirection);*/
    }

    // bool _movementChanged = false;


    private void UpdateAnimatorDirection()
    {
        // Get the normalized movement direction
        Vector3 movementDirection = _agent.velocity.normalized;

        // Calculate the angle between the agent's forward direction and the movement direction
        float angle = Vector3.SignedAngle(transform.forward, movementDirection, Vector3.up);

        // Normalize the angle to a value between -1 and 1
        float normalizedDirection = angle / 90f;

        // Only update the direction if it has changed significantly (for example, threshold of 0.1)
        if (Mathf.Abs(normalizedDirection - _previousDirection) > 0.1f)
        {
            _animController.UpdateDirection(normalizedDirection); // Update the animator with the new direction
            //animator.SetFloat("direction", normalizedDirection); // Update the direction parameter
            _previousDirection = normalizedDirection; // Store the new direction
        }

       
       
    }
    #endregion


    #region Death Region
    protected override void OwnerDeathStatusChanged(bool isDead)
    {
        base.OwnerDeathStatusChanged(isDead);

        if (OwnerIsDead)
        {
            ResetFSM(_deathState);
        }
        // Implement Respawn here
        //if (AgentIsAlive) { AgentIsAlive = false; }

        
    }

    private void DisableAgentAndObstacle()
    {
        ToggleAgent(false);
        ToggleNMObstacle(false);
    }

    protected override void ToggleNMObstacle(bool status) 
    {
        if(_obstacle.enabled == status) { return; }
        _obstacle.enabled = status;
    }

    protected override void CarveOnDestinationReached(bool reached)
    {
        //if(_currentState == _patrol) { return; }
        if (!reached) { return; }
        ToggleAgent(false);
        ToggleNMObstacle(true);
        //_obstacle.enabled = true;
    }



    private void ToggleAgent(bool agentEnabled)
    {
        if (_agent.enabled == agentEnabled) { return; }

        _agent.enabled = agentEnabled;
    }


    #endregion



    #region Global Events

    protected override void OnSceneStarted()
    {
        //InitializeWeapon();
        ChangeState(_patrol);
       
        OwnerIsDead = false;
        //AgentIsAlive = true;
        //_agent.ResetPath();
    }

    protected override void OnSceneComplete()
    {
        _animController = null;
        base.OnSceneComplete(); 
    }

    protected override void OnPlayerDeathStatusUpdated(bool isDead)
    {
        base.OnPlayerDeathStatusUpdated(isDead);
        
        //_playerIsDead = true;
        if (OwnerIsDead || !PlayerIsDead) { return; }

        
        ResetFSM(_patrol);
        
    }

    #endregion

   
}
