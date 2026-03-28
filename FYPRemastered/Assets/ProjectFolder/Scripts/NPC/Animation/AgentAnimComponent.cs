using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AgentAnimComponent : ComponentInit<ISceneAIServices, AgentEventManager>, INpcAnimationControl
{
    private AgentEventManager _em;
    private Animator _anim;
    private float dampTime = 0.1f;    // How long it takes to reach the target
    private float _speedVelocity;                      // Internal ref for SmoothDamp
    private float _directionVelocity;                  // Internal ref for SmoothDamp
    private float _currentSpeed;                       // Smoothed speed value
    private float _currentDirection;
    private float _lastDirection = float.MinValue;
    private float _lastSpeed = float.MinValue;
    private AnimationCue currentAction = AnimationCue.None;

    [Header("Anim IK Params")]
    [SerializeField] private Transform _lookTarget;


    private float _targetLookWeight = 0f;
    private float _currentLookWeight = 0f;
    private Coroutine _runningRoutine;
    [SerializeField] private float _blendSpeed = 5f;

    [Header("Weights")]
    [Range(0f, 1f)] public float lookAtWeight = 1f;
    [Range(0f, 1f)] public float bodyWeight = 0.3f;
    [Range(0f, 1f)] public float headWeight = 0.7f;
    [Range(0f, 1f)] public float eyesWeight = 0f;
    [Range(0f, 1f)] public float clampWeight = 0.5f;



    public override void Init(ISceneAIServices services, AgentEventManager manager)
    {
        _anim = GetComponent<Animator>();
        _em = manager;
    }

    public override void Unload()
    {
        _em = null;
        _anim = null;
    }


    public void UpdateBlendTreeParams(float speed, float direction)
    {
        _anim.SetFloat("speed", speed);
        _lastSpeed = speed;

        _anim.SetFloat("direction", direction);
        _lastDirection = direction;
    }



    #region Anim Triggers
    private void LookAround() => _anim.SetTrigger("look");
    private void Reload(bool isReloading) => _anim.SetBool("reloading", isReloading);
    private void DeadAnimation() => _anim.SetTrigger("dead");
    private void Shoot() => _anim.SetTrigger("shoot");
    private void MeleeAttack() => _anim.SetTrigger("melee");


    #endregion


    #region Animator Event Calls
    private void OnAnimationEventReceived(AnimationCue action)
    {
        if (currentAction != AnimationCue.None)
        {
            OnAnimationEventCompleteOrInterupted(currentAction);
        }

        currentAction = action;

        /* switch (currentAction)
         {
             case AnimationCue.Reload:
                 _enemyEventManager.Reload(action);
                // _enemyEventManager.Reloading(true);
                 break;
             case AnimationCue.Melee:
                 _enemyEventManager.MeleeTriggered(true);
                 break;
             default:
                 Debug.Log("No action specified");
                 break;
         }*/

        _em.SendAnimCue(currentAction);
    }

    private void OnAnimationEventCompleteOrInterupted(AnimationCue completedAction)
    {
        currentAction = AnimationCue.None;


        AnimationCue cueToBroadcast;
        switch (completedAction)
        {
            case AnimationCue.Reload:
                cueToBroadcast = AnimationCue.ReloadComplete;
                Reload(false);
                break;
            case AnimationCue.Melee:
                cueToBroadcast = AnimationCue.Melee;
                _em.MeleeTriggered(false);
                break;
            case AnimationCue.Switch:
                cueToBroadcast = AnimationCue.Switch;
                _anim.SetBool("Switch", false);
                break;
            default:
                cueToBroadcast = AnimationCue.None;
                Debug.Log("No action specified");
                break;
        }
        _em.SendAnimCue(cueToBroadcast);
    }
    #endregion





    #region IK Region

    private void OnAnimatorIK(int layerIndex)
    {
        if (_anim == null)
            return;

        if (_currentLookWeight <= Eps)
        {
            _anim.SetLookAtWeight(0f);
           
            return;
        }

        _anim.SetLookAtWeight(
            _currentLookWeight,
            bodyWeight,
            headWeight,
            eyesWeight,
            clampWeight
        );

        _anim.SetLookAtPosition(_lookPos);

    }
/*    private void OnAnimatorIK(int layerIndex)
    {
        if (_lookTarget == null || _anim == null)
            return;

        if (_currentLookWeight <= 0f)
        {
            _anim.SetLookAtWeight(0f);
           
            return;
        }

        _anim.SetLookAtWeight(
            _currentLookWeight,
            bodyWeight,
            headWeight,
            eyesWeight,
            clampWeight
        );

        _anim.SetLookAtPosition(_lookTarget.position);

    }*/

    public void SetIKLookTarget(Transform target) => _lookTarget = target;

   [Obsolete("", true)]
    public void IkLookAtTarget(bool look)
    {
        if (_lookTarget == null) return;
        float newTargetWeight = look ? 1f : 0f;
        if (Mathf.Approximately(_targetLookWeight, newTargetWeight)) { return; }

        _targetLookWeight = newTargetWeight;

        if (_runningRoutine != null)
        {
            StopCoroutine(_runningRoutine);
        }

        _runningRoutine = StartCoroutine(BlendLookWeight(_targetLookWeight));
    }

    private bool _cancellingLookAt = false;

    public void SetAndLookAtTarget(bool look, Transform lookTarget)
    {

        if (!look)
        {
            if (_cancellingLookAt) return;
            _cancellingLookAt = true;
        }
        else
        {
            if (lookTarget == null || lookTarget == _lookTarget) return;
            _lookTarget = lookTarget;
            _cancellingLookAt = false;
        }


        if (_lookTarget == null) return;
        float newTargetWeight = look ? 1f : 0f;
        if (Mathf.Approximately(_targetLookWeight, newTargetWeight)) { return; }

        _targetLookWeight = newTargetWeight;

        if (_runningRoutine != null)
        {
            StopCoroutine(_runningRoutine);
        }

        _runningRoutine = StartCoroutine(BlendLookWeight(_targetLookWeight));
    }

    private IEnumerator BlendLookWeight(float targetWeight)
    {
        Debug.LogError("Blending Look Weight");
        while (!Mathf.Approximately(_currentLookWeight, targetWeight))
        {
            _currentLookWeight = Mathf.Lerp(_currentLookWeight, targetWeight, Time.deltaTime * _blendSpeed);
            yield return null;
        }

        _currentLookWeight = targetWeight;
        _runningRoutine = null;
    }



    //// NEW DECEMBER 6th



    private void BlendingLayerWeightNew(AnimationLayer layer, bool activate, Action completedCB = null/*, float from, float to, float duration*/)
    {
        int index = (int)layer;
        float targetWeight = activate ? 1f : 0f;
        float currentWeight = _anim.GetLayerWeight(index);

        if (Mathf.Approximately(currentWeight, targetWeight)) { _anim.SetLayerWeight(index, targetWeight); completedCB?.Invoke(); return; }

        StartCoroutine(BlendingLayerWeightRoutine(layer, currentWeight, targetWeight, 0.5f, completedCB));
    }


    ///// END DECEMBER 6th





    private IEnumerator BlendingLayerWeightRoutine(AnimationLayer layer, float from, float to, float duration, Action completedCB = null)
    {

        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            _anim.SetLayerWeight((int)layer, Mathf.Lerp(from, to, t));
            time += Time.deltaTime;
            yield return null;
        }
        _anim.SetLayerWeight((int)layer, to);
        completedCB?.Invoke();

    }



    #endregion

    #region interface region
    public bool IsAnimationLayerActive(AnimationLayer layer) => _anim.GetLayerWeight((int)layer) == 1;
    public void ToggleAnimationLayer(AnimationLayer layer, bool activate, Action completedCB = null)
     => BlendingLayerWeightNew(layer, activate, completedCB);

   

    public void PlayClip(AnimationCue cue)
    {
        switch (cue)
        {
            case AnimationCue.Look:
                LookAround();
                break;
            case AnimationCue.Shoot:
                Shoot();
                break;
            case AnimationCue.Dead:
                DeadAnimation();
                break;
            case AnimationCue.Reload:
                Reload(true);
                break;
            case AnimationCue.Melee:
                MeleeAttack();
                break;
            default:
                Debug.LogWarning("No Animation Type Selected");
                break;
        }
    }

    public void Tick(Vector3 velocity, Vector3 forward)
    {
        if (_anim == null) return;
        velocity.y = 0f;

        // Dead-zone: treat micro-movement as zero
        if (velocity.sqrMagnitude < 0.01f)
        {
            // Smoothly return to idle/forward-facing
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, 0f, ref _speedVelocity, dampTime);
            _currentDirection = Mathf.SmoothDamp(_currentDirection, 0f, ref _directionVelocity, dampTime);
            UpdateBlendTreeParams(_currentSpeed, _currentDirection);
            return;
        }

        // Compute raw speed (magnitude) and raw direction (-1 to +1)
        float rawSpeed = velocity.magnitude;
        //Vector3 forward = transform.forward;
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
        UpdateBlendTreeParams(_currentSpeed, _currentDirection);
    }


    #endregion

    private bool _lookEnabled = false;
    private Transform _desiredTarget;
    private Transform _activeTarget;
    private Vector3 _lookPos;
    private Vector3 _lookPosVel;
    private const float Eps = 0.0001f;

    [SerializeField] private float _posSmoothTime = 0.08f;

    [Tooltip("If no target (e.g., fading out), look this far forward.")]
    [SerializeField] private float _fallbackDistance = 2f;

    public void Update()
    {
        if (_anim == null) return;

        if(!_lookEnabled && _desiredTarget == null && _currentLookWeight <= Eps)
        {
            _activeTarget = null;
            return;
        }

        float desiredWeight = (_lookEnabled && _desiredTarget != null) ? 1f : 0f;

        _currentLookWeight = Mathf.MoveTowards(_currentLookWeight, desiredWeight, Time.deltaTime * _blendSpeed);

        if(_lookEnabled && _desiredTarget != null)
            _activeTarget = _desiredTarget;
        else if (_currentLookWeight <= Eps)
            _activeTarget = null;

        if (_activeTarget == null && _currentLookWeight <= Eps)
            return;

        Vector3 desiredPos = (_activeTarget != null) ? _activeTarget.position : transform.position + transform.forward * _fallbackDistance;

        if(_posSmoothTime <= 0f)
            _lookPos = desiredPos;
        else
            _lookPos = Vector3.SmoothDamp(_lookPos, desiredPos, ref _lookPosVel, _posSmoothTime);
    }

    public void SetLookAt(bool look, Transform target = null)
    {
        _lookEnabled = look;
        _desiredTarget = look ? target : null;

        if(look && target != null)
        {
            _lookPos = target.position;
            _lookPosVel = Vector3.zero;
        }
    }

  
}

