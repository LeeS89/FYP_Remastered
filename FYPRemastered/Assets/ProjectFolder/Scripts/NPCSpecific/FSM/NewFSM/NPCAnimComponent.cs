using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCAnimComponent : ComponentEvents
{
    private EnemyEventManager _em;
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
    [SerializeField] private Transform lookTarget;

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


    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _em = eventManager as EnemyEventManager;
        _em.OnAnimationTriggered = PlayAnimationType;
        _em.OnChangeAnimatorLayerWeight = ChangeLayerWeight;
        _em.OnTargetSeen = AimTowardsTarget;
        _em.OnTickAnimator = UpdateAnimator;
        _anim = GetComponent<Animator>();
    }


    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _em.OnAnimationTriggered = null;
        _em.OnChangeAnimatorLayerWeight = null;
        _em.OnTargetSeen = null;
        _em.OnTickAnimator = null;
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

    public void UpdateAnimator(Vector3 velocity, Vector3 forward)
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

    #region Anim Triggers
    private void LookAround() => _anim.SetTrigger("look");
    private void Reload(bool isReloading) => _anim.SetBool("reloading", isReloading);
    private void DeadAnimation() => _anim.SetTrigger("dead");
    private void Shoot() => _anim.SetTrigger("shoot");
    private void MeleeAttack() => _anim.SetTrigger("melee");

    private void PlayAnimationType(AnimationCue action)
    {
        switch (action)
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
    private void AimTowardsTarget(bool targetInSight)
    {
        float newtargetWeight = targetInSight ? 1f : 0f;

        if (Mathf.Approximately(_targetLookWeight, newtargetWeight)) { return; }

        _targetLookWeight = newtargetWeight;

        if (_lookWeightCoroutine != null)
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

    private void BlendLayerWeight(AnimationLayer layer, float from, float to, float duration)
    {
        int index = (int)layer;
        float currentWeight = _anim.GetLayerWeight(index);
        to = Mathf.Clamp01(to);
        if (Mathf.Approximately(currentWeight, to) || duration <= 0f) { _anim.SetLayerWeight(index, to); return; }

        StartCoroutine(BlendLayerWeightRoutine(layer, from, to, duration));
    }


    private IEnumerator BlendLayerWeightRoutine(AnimationLayer layer, float from, float to, float duration, bool layerReady = false)
    {

        if (layer == AnimationLayer.Alert)
        {
            if (!layerReady) { _em.AimingLayerReady(false); } // Aiming animation is no longer playing -> Can no longer shoot
        }


        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            _anim.SetLayerWeight((int)layer, Mathf.Lerp(from, to, t));
            time += Time.deltaTime;
            yield return null;
        }
        _anim.SetLayerWeight((int)layer, to);

        if (layer == AnimationLayer.Alert)
        {
            if (layerReady) { _em.AimingLayerReady(true); } // Aiming animation is finished -> Can now start Shooting
        }


    }















    private void ChangeLayerWeight(AnimationLayer layer, float from, float to, float duration, bool layerReady = false)
       => StartCoroutine(FadeLayerWeight(layer, from, to, duration, layerReady));

    private IEnumerator FadeLayerWeight(AnimationLayer layer, float from, float to, float duration, bool layerReady = false)
    {

        if (layer == AnimationLayer.Alert)
        {
            if (!layerReady) { _em.AimingLayerReady(false); } // Aiming animation is no longer playing -> Can no longer shoot
        }

        
        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            _anim.SetLayerWeight((int)layer, Mathf.Lerp(from, to, t));
            time += Time.deltaTime;
            yield return null;
        }
        _anim.SetLayerWeight((int)layer, to);

        if (layer == AnimationLayer.Alert)
        {
            if (layerReady) { _em.AimingLayerReady(true); } // Aiming animation is finished -> Can now start Shooting
        }


    }
   
    #endregion



}

public enum AnimationLayer
{
    Alert = 1,
    Combat = 2,
    LookAround = 3
}
