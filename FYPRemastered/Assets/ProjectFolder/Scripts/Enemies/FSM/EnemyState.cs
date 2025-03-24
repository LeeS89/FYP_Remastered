using System;
using UnityEngine;

public abstract class EnemyState
{
    public event Action<AnimationAction> OnAnimationTriggered;
    public event Action<float, float> OnSpeedChanged;

    protected EnemyFSMController _fsm;
    protected Coroutine _coroutine;

    public EnemyState(EnemyFSMController fsm)
    {
        _fsm = fsm;
    }

    public void AnimationTriggered(AnimationAction action)
    {
        OnAnimationTriggered?.Invoke(action);
    }

    public void SpeedChanged(float speed, float lerpSpeed)
    {
        OnSpeedChanged?.Invoke(speed, lerpSpeed);
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void OnStateDestroyed();
}
