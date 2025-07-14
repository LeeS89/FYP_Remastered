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

        _enemyEventManager.Melee(inProgress);
    }

    public void OnReloadComplete(int reloadState)
    {
        // 0 = reload ended, 1 = reload started
        bool isReloading = reloadState == 1;

        _enemyEventManager.Reload(isReloading);
        /* int clampedValue = Mathf.Clamp(reloadingInt, 0, 1);
         bool isReloading = reloadingInt != 0;

         _enemyEventManager.Reload(isReloading);*/
    }

    public void DeathAnimationComplete()
    {
        _enemyEventManager.DeathAnimationComplete();
    }
}
