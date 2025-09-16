using System.Collections;
using System.Xml;
using UnityEngine;

public class EnemyAnimationEvents : ComponentEvents
{
    private EnemyEventManager _enemyEventManager;

    [SerializeField] private Transform lookTarget; // Pull from event later
    [SerializeField] private float maxPitchAngle = 30f;
    [SerializeField] private float aimSpeed = 5f;

    private Animator animator;
   
   
    private float _targetLookWeight = 0f;
    private float _currentLookWeight = 0f;
    private Coroutine _lookWeightCoroutine;
    [SerializeField] private float _blendSpeed = 5f;

    [Header("Weights")]
    [Range(0f, 1f)] public float lookAtWeight = 1f;
    [Range(0f, 1f)] public float bodyWeight = 0.3f;
    [Range(0f, 1f)] public float headWeight = 0.7f;
    [Range(0f, 1f)] public float eyesWeight = 0f;
    [Range(0f, 1f)] public float clampWeight = 0.5f;


    public Vector3 _weaponPos;
    public GameObject _weaponObject;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _enemyEventManager = eventManager as EnemyEventManager;
        base.RegisterLocalEvents(_enemyEventManager);
        _enemyEventManager.OnTargetSeen += AimTowardsTarget;
        animator = GetComponent<Animator>();

        RegisterGlobalEvents();

    }
   
    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(eventManager);
        base.UnRegisterGlobalEvents();
        _enemyEventManager.OnTargetSeen -= AimTowardsTarget;
    }

    protected override void RegisterGlobalEvents()
    {
        base.RegisterGlobalEvents();
        GameManager.OnPlayerDeathStatusChanged += OnPlayerDeathStatusUpdated;
    }

    protected override void UnRegisterGlobalEvents()
    {
        base.UnRegisterGlobalEvents();
        GameManager.OnPlayerDeathStatusChanged -= OnPlayerDeathStatusUpdated;
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        animator = null;
        _enemyEventManager = null;
    }

    public bool ShouldAim = true;

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || lookTarget == null)
            return;

        animator.SetLookAtWeight(
            _currentLookWeight,
            bodyWeight,
            headWeight,
            eyesWeight,
            clampWeight
        );

        animator.SetLookAtPosition(lookTarget.position);
       
    }

  

    public bool _testMelee = false;

    public void SetMelee()
    {
        ShouldAim = false;
    }

    public void ResetMelee()
    {
        ShouldAim = true;
    }

    public void MeleeAttackPerformed()
    {
        _enemyEventManager.MeleeAttackPerformed();
    }

    private void AimTowardsTarget(bool targetInSight)
    {
        float newtargetWeight = targetInSight ? 1f : 0f;

        if(Mathf.Approximately(_targetLookWeight, newtargetWeight)) { return; }

        _targetLookWeight = newtargetWeight;

        if(_lookWeightCoroutine != null)
        {
            StopCoroutine(_lookWeightCoroutine);
        }

        _lookWeightCoroutine = StartCoroutine(BlendLookWeight(_targetLookWeight));
    }

    private IEnumerator BlendLookWeight(float targetWeight)
    {
        while (!Mathf.Approximately(_currentLookWeight, targetWeight))
        {
            _currentLookWeight = Mathf.Lerp(_currentLookWeight, targetWeight, Time.deltaTime * _blendSpeed);
            yield return null;
        }

        _currentLookWeight = targetWeight;
        _lookWeightCoroutine = null;
    }

   

    public void OnShoot()
    {
        _enemyEventManager.FireRangedWeapon();
    }

    public void MeleeTriggered(int meleeState)
    {
        bool inProgress = meleeState == 1;

        _enemyEventManager.MeleeTriggered(inProgress);
    }

    public void OnReloadComplete(int reloadState)
    {
        // 0 = reload ended, 1 = reload started
        bool isReloading = reloadState == 1;

        //_enemyEventManager.Reload(isReloading);
        /* int clampedValue = Mathf.Clamp(reloadingInt, 0, 1);
         bool isReloading = reloadingInt != 0;

         _enemyEventManager.Reload(isReloading);*/
    }

    public void DeathAnimationComplete()
    {
        _enemyEventManager.DeathAnimationComplete();
    }

   /* public void FadeLookLayer()
    {
        _enemyEventManager.ChangeAnimatorLayerWeight(EnemyAnimController.AnimationLayer.LookAround, 1, 0, 0.5f);
    }*/

    public AnimationAction currentAction = AnimationAction.None;


    private void OnAnimationEventReceived(AnimationAction action)
    {
        if (currentAction != AnimationAction.None)
        {
            OnAnimationEventCompleteOrInterupted(currentAction);
        }

        currentAction = action;

        switch (action)
        {
            case AnimationAction.Reload:
                _enemyEventManager.Reload(true);
                break;
            case AnimationAction.Melee:
                _enemyEventManager.MeleeTriggered(true);
                break;
            default:
                Debug.Log("No action specified");
                break;
        }


    }

    private void OnAnimationEventCompleteOrInterupted(AnimationAction completedAction)
    {
        currentAction = AnimationAction.None;

        switch (completedAction)
        {
            case AnimationAction.Reload:
                _enemyEventManager.Reload(false);
                animator.SetBool("reloading", false);
                break;
            case AnimationAction.Melee:
                _enemyEventManager.MeleeTriggered(false);
                break;
            case AnimationAction.Switch:
                animator.SetBool("Switch", false);
                break;
            default:
                Debug.Log("No action specified");
                break;
        }
    }

    public bool _testSwitch = false;
    private void Update()
    {
        if (_testSwitch)
        {
            _weaponObject.transform.localPosition = _weaponPos;
            _weaponObject.SetActive(!_weaponObject.activeSelf);
            _testSwitch = false;
           // animator.SetBool("Switch", true);
           
        }
    }
}
