using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class FSMPatrolState : FSMBaseState
{
    public FSMPatrolState(IAgentData data, IPathResolver resolver) : base(data, resolver) { }
    

    public override void EnterState()
    {
        throw new System.NotImplementedException();
    }

    public override void ExitState()
    {
        throw new System.NotImplementedException();
    }

    public override void OnDestinationReached()
    {
        throw new System.NotImplementedException();
    }

    private IEnumerator PatrolWaitRoutine(Transform t, float minWait, float maxWait, Vector3? forward, Action<AnimationCue> animationCB, Func<StateId, bool> cantContinueCB, Action<StateId> OnDone)
    {
        Debug.LogError("Patrol wait routine called");
        if (forward != null)
        {
            Quaternion targetRot = Quaternion.LookRotation(forward.Value);
            while (Quaternion.Angle(t.rotation, targetRot) > 2.0f + Mathf.Epsilon)
            {
                t.rotation = Quaternion.Slerp(t.rotation, targetRot, Time.deltaTime * 2f);
                yield return null;
            }

        }
        if (!ContinueRoutine) yield break;

        animationCB?.Invoke(AnimationCue.Look);
       
        float _delayTime = Random.Range(minWait, maxWait);
        float elapsedTime = 0.0f;

        while (elapsedTime < _delayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (!ContinueRoutine) yield break;
        /*OnDone?.Invoke(id);*/ // Continue to next destination

    }
}
