using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public static class FSMExtension
{
    public static Coroutine BeginPatrolRoutine(this FSMManager m, StateId id, Transform t, float minWait, float maxWait, Vector3? forward, Action<AnimationCue> animationCB, Func<StateId, bool> canContinueCB, Action<StateId> OnDone)
        => CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutine(id, t, minWait, maxWait, forward, animationCB, canContinueCB, OnDone));

    private static IEnumerator PatrolWaitRoutine(StateId id, Transform t, float minWait, float maxWait, Vector3? forward, Action<AnimationCue> animationCB, Func<StateId, bool> cantContinueCB, Action<StateId> OnDone)
    {
        Debug.LogError("Patrol wait routine called");
        if (forward != null)
        {
            Quaternion targetRot = Quaternion.LookRotation(forward.Value);
            while (Quaternion.Angle(t.rotation, targetRot) > 2.0f + Mathf.Epsilon)
            {
                if (cantContinueCB?.Invoke(id) ?? false) yield break;
                t.rotation = Quaternion.Slerp(t.rotation, targetRot, Time.deltaTime * 2f);
                yield return null;
            }

        }
        if (cantContinueCB?.Invoke(id) ?? false) yield break;

        animationCB?.Invoke(AnimationCue.Look);
        //Owner.OwnerEM.TriggerAnimation(AnimationCue.Look);
        if (cantContinueCB?.Invoke(id) ?? false) yield break;
      //  if (_currentStateId != StateId.Patrol) { _runningRoutine = null; yield break; }

        float _delayTime = Random.Range(minWait, maxWait);
        float elapsedTime = 0.0f;

        while (elapsedTime < _delayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (cantContinueCB?.Invoke(id) ?? false) yield break;
        OnDone?.Invoke(id);
       
    }
}
