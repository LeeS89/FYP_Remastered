using System.Xml;
using UnityEngine;

public class EnemyAnimationEvents : ComponentEvents
{
    private EnemyEventManager _enemyEventManager;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _enemyEventManager = _eventManager as EnemyEventManager;
    }

    public void OnShoot()
    {
        _enemyEventManager.Shoot();
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
                break;
            case AnimationAction.Melee:
                _enemyEventManager.MeleeTriggered(false);
                break;
            default:
                Debug.Log("No action specified");
                break;
        }
    }
}
