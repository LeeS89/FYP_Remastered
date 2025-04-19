using UnityEngine;
using UnityEngine.AI;

public class DeathState : EnemyState
{
    private bool _deathAnimationComplete = false;
    private float _timeToDisable = 3;
    private float _countdown;

    public DeathState(EnemyEventManager eventManager, GameObject owner) : base(eventManager, owner) { }
    

    public override void EnterState(AlertStatus alertStatus = AlertStatus.None, float _ = 0)
    {
        _eventManager.OnDeathAnimationComplete += DeathAnimationComplete;
        _eventManager.AnimationTriggered(AnimationAction.Dead);
        _deathAnimationComplete = false;
        _countdown = _timeToDisable;
    }

    public override void ExitState()
    {
        _eventManager.OnDeathAnimationComplete -= DeathAnimationComplete;
    }

    private void DeathAnimationComplete()
    {
        _deathAnimationComplete = true; // This is called on the last frame of the death animation via animation event
    }

    public override void LateUpdateState()
    {
        if (!_deathAnimationComplete) { return; }

        if(_countdown > 0)
        {
            _countdown -= Time.deltaTime;
        }
        else
        {
            _deathAnimationComplete = false;
            _eventManager.DeathComplete(false); // Passing false here disables the game object once the death sequence completes
        }
    }
}
