using System.Collections;
using UnityEngine;

public class EnemyAnimController
{
    private Animator _anim;
    private EnemyEventManager _eventManager;

    public EnemyAnimController(Animator anim, EnemyEventManager eventManager)
    {
        _anim = anim;
        _eventManager = eventManager;
        BindToEvents();
    }

    private void BindToEvents()
    {
        if(_eventManager == null)
        {
            Debug.LogError("No Valid Event Manager Passed");
            return;
        }
        _eventManager.OnAnimationTriggered += PlayAnimationType;
        _eventManager.OnChangeAnimatorLayerWeight += ChangeLayerWeight;
    }

    public void SetAlertStatus(bool alert)
    {
        _anim.SetBool("alert", alert);
    }

   /* public void EnemyDied()
    {
        _anim.SetTrigger("dead");
    }*/

    public void UpdateSpeed(float speed)
    {
        _anim.SetFloat("speed", speed);
    }

    public void LookAround()
    {
        _anim.SetTrigger("look");
       
    }

    public void Reload()
    {
        _anim.SetTrigger("reload");
    }


    public void DeadAnimation()
    {
        _anim.SetTrigger("dead");
    }

    public void Shoot()
    {
        _anim.SetTrigger("shoot");
    }

   

    private void PlayAnimationType(AnimationAction action)
    {
        switch (action)
        {
            case AnimationAction.Look:
                LookAround();
                break;
            case AnimationAction.Shoot:
                Shoot();
                break;
            case AnimationAction.Dead:
                DeadAnimation();
                break;
            case AnimationAction.Reload:
                Reload();
                break;
            default:
                Debug.LogWarning("No Animation Type Selected");
                break;
        }
    }


    private void ChangeLayerWeight(int layer, float from, float to, float duration, bool layerReady)
    {
        CoroutineRunner.Instance.StartCoroutine(FadeLayerWeight(layer, from, to, duration, layerReady));
    }

    private IEnumerator FadeLayerWeight(int layer, float from, float to, float duration, bool layerReady)
    {
        if (!layerReady) { _eventManager.AimingLayerReady(false); } // Aiming animation is no longer playing -> Can no longer shoot

        float time = 0f;
        while(time < duration)
        {
            float t = time / duration;
            _anim.SetLayerWeight(layer, Mathf.Lerp(from, to, t));
            time += Time.deltaTime;
            yield return null;
        }
        _anim.SetLayerWeight(layer, to);

        if (layerReady) { _eventManager.AimingLayerReady(layerReady); } // Aiming animation is finished -> Can now start Shooting
    }
}
