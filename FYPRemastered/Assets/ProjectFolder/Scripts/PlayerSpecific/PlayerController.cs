using Oculus.Interaction.HandGrab;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerEventManager))]
public sealed class PlayerController : TargetableInit<IPlaceholderService, PlayerEventManager>/*ComponentEvents*///, ITargetable
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
    private PlayerEventManager _eManager;
    private LocomotionHandler _locomotion;
    private RotationHandler _rotationHandler;
    private GrabHandler _grabHandler;
    public bool InputEnabled { get; private set; } = false;

 //   [Header("ITargetable Info - Used by NPC's for targeting/ FOV purposes")]
 //   [SerializeField] private Collider _targetableCollider;
   // [SerializeField] private LayerMask _selfTargetMask;


    #region ITargetable Implementation

   // public Vector3 Forward => transform.forward;

  /*  [SerializeField] private Transform _rootTransform;
    public Transform Transform => _rootTransform == null ? transform : _rootTransform;*/

  //  public Collider TargetableCollider => _targetableCollider;

    public bool _testMove = true;
   // public bool IsStationary => _testMove;//_locomotion != null ? !_locomotion.CanMoveForward : true;

   // public bool IsDead { get; private set; } = false;

   // public LayerMask LayerMask => _selfTargetMask;

   /* public Vector3 Position()
    => _rootTransform == null ? transform.position : _rootTransform.position;

    public Quaternion Rotation()
    {
        throw new System.NotImplementedException();
    }*/
    #endregion ITargetable Implementation

    public /*override*/ void RegisterLocalEvents(EventManager eventManager)
    {
       // _eManager = eventManager as PlayerEventManager;
    
        //base.RegisterLocalEvents(_eManager);

        if (TryGetComponent<CharacterController>(out CharacterController characterController))
            _controller = characterController;

     //   if(_targetableCollider == null)
        //{
            Collider col = GetComponentInChildren<Collider>();
           // if (col != null)
             //   _targetableCollider = col;
          //  else
            //    _targetableCollider = gameObject.AddComponent<BoxCollider>();
       // }

        _eManager.OnPlayerRotate += HandleRotation;
        _eManager.OnPlayerHeightUpdated += AdjustPlayerHeight;
        _eManager.OnMovementUpdated += ApplyPlayerMovement;
       // _playerEventManager.OnDeathStatusUpdated += DeathStatusUpdated;

        _grabHandler = new GrabHandler(_eManager, GetComponentsInChildren<HandGrabInteractor>(false));
        _locomotion = new LocomotionHandler(_eManager, transform, _moveSpeed, _gravity);
       
        SetupRotationHandler();

       // RegisterGlobalEvents();
    }

    public override void Init(IPlaceholderService services, PlayerEventManager manager)
    {
        _eManager = manager;
        if (TryGetComponent<CharacterController>(out CharacterController characterController))
            _controller = characterController;

       /* if (_targetableCollider == null)
        {
            Collider col = GetComponentInChildren<Collider>();
            if (col != null)
                _targetableCollider = col;
            else
                _targetableCollider = gameObject.AddComponent<BoxCollider>();
        }*/

        _eManager.OnPlayerRotate += HandleRotation;
        _eManager.OnPlayerHeightUpdated += AdjustPlayerHeight;
        _eManager.OnMovementUpdated += ApplyPlayerMovement;

        _grabHandler = new GrabHandler(_eManager, GetComponentsInChildren<HandGrabInteractor>(false));
        _locomotion = new LocomotionHandler(_eManager, transform, _moveSpeed, _gravity);

        SetupRotationHandler();
    }


    private void SetupRotationHandler()
    {
        var cfg = new RotationHandler.Config(_camera, transform, _rotationSpeed, _rotationThreshold, _stopRotationThreshold);
        _rotationHandler = new RotationHandler(_eManager, cfg);
    }

   /* public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(_eManager);
        //_playerEventManager.OnDeathStatusUpdated -= DeathStatusUpdated;
        _eManager.OnMovementUpdated -= ApplyPlayerMovement;
        _eManager.OnPlayerRotate -= HandleRotation;
        _eManager.OnPlayerHeightUpdated -= AdjustPlayerHeight;
        base.UnRegisterGlobalEvents();
    }*/

    public override void Unload()
    {
        _eManager.OnMovementUpdated -= ApplyPlayerMovement;
        _eManager.OnPlayerRotate -= HandleRotation;
        _eManager.OnPlayerHeightUpdated -= AdjustPlayerHeight;
    }

    private Vector3 _lastPosition;
    public float movementThreshold = 0.01f;
    public bool _testTrim = false;

    public bool _printStatStatus = false;

    private void Update()
    {
        if (_testTrim)
        {
            ComponentRegistry.TrimAll();
            _testTrim = false;
        }

        if (!InputEnabled) { return; }

        _locomotion?.Tick(_controller.isGrounded);

#if UNITY_EDITOR
        float movedDistance = Vector3.Distance(Position(), _lastPosition);

        IsStationary = movedDistance <= movementThreshold;

        if (_printStatStatus)
            Debug.LogError("IsStationary is: "+IsStationary);
       /* if (movedDistance > movementThreshold)
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
        }*/

        _lastPosition = Position();
#endif

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
        if (_controller == null || IsDead/*OwnerIsDead*/) { return; }

        _controller.Move(velocity * Time.deltaTime);

    }

    protected override void OnSceneBegin()
        => InputEnabled = true;

    protected override void OnSceneEnd()
    {
        InputEnabled = false;
        _locomotion?.OnInstanceDestroyed();
        _locomotion = null;
        _rotationHandler?.OnInstanceDestroyed();
        _rotationHandler = null;

        _grabHandler.OnInstanceDestroyed();
        _grabHandler = null;
        _eManager = null;
    }
    /*protected override void OnSceneStarted()
    {
        base.OnSceneStarted();
        InputEnabled = true;
    }*/

   
    private/*protected*/ /*override*/ void DeathStatusUpdated(bool isDead)
    {
       // base.DeathStatusUpdated(isDead);

      //  InputEnabled = !OwnerIsDead;
       
    }

   /* protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        InputEnabled = false;
        _locomotion?.OnInstanceDestroyed();
        _locomotion = null;
        _rotationHandler?.OnInstanceDestroyed();
        _rotationHandler = null;
       
        _grabHandler.OnInstanceDestroyed();
        _grabHandler = null;
        _eManager = null;
    }*/

  

    
}
