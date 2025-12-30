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

    public void SetIKLookTarget(Transform target) => _lookTarget = target;

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

}

