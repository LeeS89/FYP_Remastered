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
}
