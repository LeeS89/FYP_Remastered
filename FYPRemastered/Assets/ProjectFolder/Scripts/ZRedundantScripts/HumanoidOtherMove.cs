using ProjectDawn.Navigation.Hybrid;
using System;
using Unity.Mathematics;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class HumanoidOtherMove : MonoBehaviour
{
    float lookahead = 0.25f;   // seconds of anticipation
    float minRadius = 0.3f;


    private Rigidbody _rb;
    public WaypointManager _wpManager;

    public CircleManager _circleManager;
    private DestinationBase _destinationProvider;

    [SerializeField] private NavMeshAgent _agent;
     private NavMeshPath _path;
    [SerializeField] private AgentAuthoring _agentAuth;

    // [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _maxSpeed = 3.5f;

    [SerializeField] private float _slowingRadius = 1f;
    [SerializeField] private float _arrivalThreshold = 0.2f;
    [SerializeField] private float _stopRange = 0f;
    private Coroutine _runningRoutine = null;

    private Vector3 _currentVelocity;

    [Obsolete]
    [SerializeField] private float _steerForce = 8f;
    [SerializeField] private float _accel = 8f;
    [SerializeField] private float _decel = 8f;

    public DestinationMode _destinationMode = DestinationMode.Waypoint;

    private bool _hasDestination = false;
    public Animator _anim;

    public float _turnSpeed = 5f;

    public Material _originalMat;
    public Material _waitingMat;

    private void Awake()
    {
        if (!TryGetComponent(out _rb))
            _rb = GetComponentInChildren<Rigidbody>();

        if (!TryGetComponent(out _agent))
            _agentAuth = GetComponentInChildren<AgentAuthoring>();

       // _path = new NavMeshPath();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        
        /*_agent.updatePosition = false;
        _agent.updateRotation = false;*/

        /*if (_destinationMode == DestinationMode.Waypoint)
            _destinationProvider = new WaypointDestination(_wpManager, _agent);
        else if (_destinationMode == DestinationMode.Random)
            _destinationProvider = new RandomDestination(_agent);
        else*/ if (_destinationMode == DestinationMode.Circle)
            _destinationProvider = new CircleDestination(_circleManager, _agentAuth);

        if (_destinationProvider == null) return;
        _destinationProvider.Init();

        if (_destinationMode == DestinationMode.Circle)
        {
            Vector3 startPos = _destinationProvider.GetWaypointPositionOnNavMesh();
            _agent.transform.position = startPos;
            //if (!_agent.Warp(startPos)) Debug.LogError("Failed to teleport");

        }

        TrySetDestination();

    }

    //  public bool _usingWaitChance = false;
    private Vector3 _currentDest;
    private Vector3? _prevDest = null;

    public bool _ignoringWaitChance = false;

    private void TrySetDestination()
    {
        if (_destinationProvider?.TryGetPath(out _currentDest) is true)
        {
        
            //_hasDestination = _agent.SetPath(_path);
            if (_hasDestination)
            {
                if (!_ignoringWaitChance)
                {
                    int chanceOfWaitAtNextPoint = Random.Range(0, 10);
                    _waitAtNextPoint = chanceOfWaitAtNextPoint < 5;
                    var locomotion = _agentAuth.EntityLocomotion;
                    locomotion.AutoBreaking = true;
                    _agentAuth.EntityLocomotion = locomotion;

                    float t = _agentAuth.EntityBody.RemainingDistance; 
                    //_agent.autoBraking = _waitAtNextPoint;
                }
                _maxRadius = _agentAuth.EntityBody.RemainingDistance;//_agent.remainingDistance;
                /*
                                TestVisualWPToggle(_waitAtNextPoint ? _waitingMat : _originalMat, _currentDest);
                                TestVisualWPToggle(_originalMat, _prevDest);*/
            }

        }

    }
    private void TrySetDestination(Vector3 destination)
    {
        _hasDestination = _agent.SetPath(_path);
        if (_hasDestination)
        {
            int chanceOfWaitAtNextPoint = Random.Range(0, 10);
            _waitAtNextPoint = chanceOfWaitAtNextPoint > 10;
        }

    }

    protected bool HasClearPathToTarget(Vector3 from, Vector3 to)
    {

        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, _path))
            return false;

        return _path.status == NavMeshPathStatus.PathComplete;
    }

    protected bool TryGetNearestPointOnNavMesh(Vector3 samplePoint, /*out Vector3 resolvedPoint, */float maxDistance)
    {
        // resolvedPoint = default;
        if (NavMesh.SamplePosition(samplePoint, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            // resolvedPoint = hit.position;
            return HasClearPathToTarget(_agent.transform.position, hit.position);
        }
        return false;
    }

    private bool DestinationReachedNew(out float remaining)
    {
        remaining = 0f;
        if (_agent.pathPending || !_agent.hasPath)
            return false;

        // Use the actual last corner point to avoid internal NavMesh lag
        Vector3[] corners = _agent.path.corners;
        if (corners.Length == 0)
            return false;

        Vector3 finalDestination = corners[corners.Length - 1];
        float distanceToTarget = Vector3.Distance(_agent.transform.position, finalDestination);
        remaining = distanceToTarget - _stopRange;
        // Dynamic threshold: scale it slightly with velocity if needed, or use a flat radius
        float effectiveThreshold = _arrivalThreshold + (_currentVelocity.magnitude * Time.fixedDeltaTime);

        if ((distanceToTarget - _stopRange) <= effectiveThreshold)
        {
            _hasDestination = false;
            _agent.ResetPath();
            _prevDest = _currentDest; //////////////////////////////////////////////////////
            return true;
        }

        return false;
    }


 /*   private bool DestinationReached()
    {
        if (_agent.pathPending || !_agent.hasPath)
            return false;
        if ((_agent.remainingDistance - _stopRange) <= _arrivalThreshold)
        {
            _hasDestination = false;
            _agent.ResetPath();
            return true;

        }
        return false;
    }*/

    float _maxRadius;

    private bool DestinationReachedNewest()
    {
        if (_agent.pathPending || !_agent.hasPath)
            return false;


        //float switchRadius = Mathf.Max(minRadius, _agent.velocity.magnitude * lookahead);
        float v = _agent.velocity.magnitude;
        float radius = MathF.Max(0.3f, 0.5f * v * v / _agent.acceleration);
        radius = Mathf.Min(radius, _maxRadius);


        if ((_agent.remainingDistance - _stopRange) <= radius)
        {
            _hasDestination = false;
            _agent.ResetPath();
            return true;

        }
        return false;
    }


    bool _waitAtNextPoint = false;

   /* void UpdateLd()
    {

        if (DestinationReached())
        {

            if (_waitAtNextPoint)
            {
                if (_runningRoutine == null)
                {
                    _waitAtNextPoint = false;
                    _runningRoutine = StartCoroutine(WaitDelay());
                    return;
                }
            }
            TrySetDestination();

        }

        Vector3 target = _agent.desiredVelocity;
        float rate = target.sqrMagnitude > _currentVelocity.sqrMagnitude ? _accel : _decel;
        _currentVelocity = Vector3.MoveTowards(_currentVelocity, target, rate * Time.deltaTime);

        _agent.velocity = _currentVelocity;
        Vector3 horizontalVelocity = new Vector3(_agent.velocity.x, 0, _agent.velocity.z);
        float cSpeed = horizontalVelocity.magnitude;
        Debug.LogError($"Current speed from velocity: {cSpeed}");

        Vector3 flat = _currentVelocity;
        flat.y = 0f;

        if (flat.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_currentVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);


        }
    }
*/



    private void Update()
    {
        /*return; // For testing
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
      
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {

                if (TryGetNearestPointOnNavMesh(hit.point, 5f))
                    TrySetDestination(Vector3.zero);

            }
        }*/
    }



    [SerializeField] private float _grip = 270f;


    private void TestVisualWPToggle(Material mat, Vector3? destination/* = null*/)
    {
        if (!destination.HasValue) return;
        Collider[] cols = Physics.OverlapSphere(destination.Value, 0.5f);

        foreach (Collider col in cols)
        {
            if (col == null || !col.gameObject.CompareTag("Threat")) continue;
            MeshRenderer rend = col.gameObject.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                rend.material = mat;

                /*if(rend.enabled) rend.enabled = false;
                else
                    rend.enabled = true;*/
            }
        }
    }

    void FixedUpdate()
    {
        if (_agent == null || _anim == null) return;

        float remainingdist;
        if (DestinationReachedNewest())
        {

            if (_waitAtNextPoint)
            {
                if (_runningRoutine == null)
                {

                    _runningRoutine = StartCoroutine(WaitDelay());

                }
            }
            else
                TrySetDestination();

        }


        Vector3 currentVel = _agent.velocity;
        Vector3 horizontalVelocity = new Vector3(currentVel.x, 0, currentVel.z);
        float animSpeed = horizontalVelocity.magnitude;

        _anim.SetFloat("Speed", animSpeed, 0.1f, Time.fixedDeltaTime);


        return;
        _agent.nextPosition = _rb.position;
        //float remainingdist;
        if (DestinationReachedNew(out remainingdist))
        {

            if (_waitAtNextPoint)
            {
                if (_runningRoutine == null)
                {

                    _runningRoutine = StartCoroutine(WaitDelay());

                }
            }
            else
                TrySetDestination();

        }

        if (!_hasDestination && _currentVelocity.sqrMagnitude <= 0.0001f)
        {
            _currentVelocity = Vector3.zero;
            _anim.SetFloat("Speed", 0f, 0.1f, Time.fixedDeltaTime);
            return;
        }
        _agent.nextPosition = _rb.position;



        // Get the direction the agent wants to move in
        // if _agent.desiredVelocity.sqrMagnitude is any smaller than 0.001f it means the direction effectivly has no length so dont move
        Vector3 desiredDir = _agent.desiredVelocity.sqrMagnitude > 0.001f
            ? _agent.desiredVelocity.normalized
            : Vector3.zero;

        // if the agent is currently moving along a path or just received a path
        // remaining becomes the max of actual agent.remainingDistance and 0 => in case actual agent.remainingdistance becomes negative
        // We would use 0 remaining in that case, meaning we have arrived
        // Otherwise, we dont have a path so stay where we are
        float remaining = (_agent.hasPath && !_agent.pathPending)
            ? Mathf.Max(/*_agent.remainingDistance - _stopRange*/remainingdist, 0f)
            : 0f;

        // If we are not going to stop at the next waypoint, we can just set targetSpeed to max speed
        // Otherwise, target speed scales depending on remaining distance to destination
        // capping it at max speed for huge distances left
        // Mathf.Sqrt(2f * _decel * remaining) answers, what is the max speed i can move at and still stop in time at the destination
        float targetSpeed = (_waitAtNextPoint)
             ? Mathf.Min(_maxSpeed, Mathf.Sqrt(2f * _decel * remaining))
              : _maxSpeed;


        // Aligment essentially means how sharp/ soft the direction change is if any
        // And the sharper the turn, the more we slow down
        // Dot == 1 means no turn at all so continue on at current / full speed
        // With a Dot of -1 meaning the sharpest possible turn (exact opposite direction) so slow to max 15% speed
        // Dot of 0 means perpendicular, so slow to max 37% speed
        // InverseLerp clamps to 0 when alignment is < -0.3 and to 1 when > 0.85, otherwise the fraction of the way from -0.3 to 0.85
        // the result of InverseLerp e.g. 0.7 represents a value of 70% of the way between the Lerp values of 0.15 and 1
        // In this case, 70% of the way would be 0.74 => So speed scales to 74% of targetSpeed
        float alignment = Vector3.Dot(transform.forward, desiredDir);
        targetSpeed *= Mathf.Lerp(0.15f, 1f, Mathf.InverseLerp(-0.3f, 0.85f, alignment));

        Vector3 targetVel = desiredDir * targetSpeed;


        // Calculate heading
        // Current direction will be transform.forward if we are not moving
        // Otherwise, it will be _currentVelocity.Normalized
        // new aim direction will be desiredDir if it has a length of > 0.001f, otherwise currentDirection
        float curSpeed = _currentVelocity.magnitude;
        Vector3 curDir = curSpeed > 0.05f ? _currentVelocity / curSpeed : transform.forward;
        Vector3 aimDir = desiredDir.sqrMagnitude > 0.001f ? desiredDir : curDir;

        // Rotate from current direction to aim direction _grip degrees per second
        Vector3 newDir = Vector3.RotateTowards(curDir, aimDir,
            _grip * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);

        // Rate => Should we use the accelerate or decelerate multiplier?
        // Move our current speed towards the target speed at a speed of rate per second
        float rate = targetSpeed > curSpeed ? _accel : _decel;
        float newSpeed = Mathf.MoveTowards(curSpeed, targetSpeed, rate * Time.fixedDeltaTime);

        _currentVelocity = newDir * newSpeed;

        // Get magnitude for nimator blend tree
        // Vector3 horizontalVelocity = new Vector3(_currentVelocity.x, 0, _currentVelocity.z);
        /*  float animSpeed = horizontalVelocity.magnitude;

          _anim.SetFloat("Speed", animSpeed, 0.1f, Time.fixedDeltaTime);

          // Applying movement
          Vector3 newPos = _rb.position + _currentVelocity * Time.fixedDeltaTime;
          _rb.MovePosition(newPos);
          _agent.nextPosition = newPos;


          // ---- facing: don't rotate away from our motion until we've slowed down 
          Vector3 velDir = _currentVelocity.sqrMagnitude > 0.01f
              ? _currentVelocity.normalized
              : transform.forward;

          // how much of a reversal is the new direction? 0 = ahead, 1 = fully behind us
          float behind = Mathf.Clamp01(-Vector3.Dot(velDir, desiredDir));

          // how fast are we still moving? 0 = crawling, 1 = quick
          float fast = Mathf.InverseLerp(0.3f, _maxSpeed * 0.5f, _currentVelocity.magnitude);

          // while braking out of a reversal, face where we're GOING; otherwise face the path
          Vector3 lookDir = Vector3.Slerp(desiredDir, velDir, behind * fast);
          lookDir.y = 0f;


          if (lookDir.sqrMagnitude > 0.001f)
          {
              Quaternion look = Quaternion.LookRotation(lookDir);
              _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, look,
                  1f - Mathf.Exp(-_turnSpeed * Time.fixedDeltaTime)));
          }
  */
    }


    private void FixedUpdateLd()
    {
        /*if (DestinationReached())
        {

            if (_waitAtNextPoint)
            {
                if (_runningRoutine == null)
                {
                    _waitAtNextPoint = false;
                    _runningRoutine = StartCoroutine(WaitDelay());
                    return;
                }
            }
            TrySetDestination();

        }*/

        Vector3 target = _agent.desiredVelocity;
        float rate = target.sqrMagnitude > _currentVelocity.sqrMagnitude ? _accel : _decel;
        _currentVelocity = Vector3.MoveTowards(_currentVelocity, target, rate * Time.fixedDeltaTime);

        // _agent.velocity = _currentVelocity;

        _rb.MovePosition(_rb.position + _currentVelocity * Time.fixedDeltaTime);

        Vector3 horizontalVelocity = new Vector3(_agent.velocity.x, 0, _agent.velocity.z);
        float cSpeed = horizontalVelocity.magnitude;
        Debug.LogError($"Current speed from velocity: {cSpeed}");
        _anim.SetFloat("Speed", cSpeed);


        Vector3 flat = _currentVelocity;
        flat.y = 0f;

        if (flat.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_currentVelocity);
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime));
            /*Quaternion targetRotation = Quaternion.LookRotation(_currentVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime);*/


        }

        _agent.nextPosition = _rb.position;
    }

    public bool _amIWaiting = false;
    private IEnumerator WaitDelay()
    {
        _amIWaiting = true;
        yield return new WaitForSeconds(3f);
        _amIWaiting = false;
        TrySetDestination();
        // _waitAtNextPoint = false;
        _runningRoutine = null;
    }
}

public enum DestinationMode
{
    Waypoint,
    Random,
    Circle
}