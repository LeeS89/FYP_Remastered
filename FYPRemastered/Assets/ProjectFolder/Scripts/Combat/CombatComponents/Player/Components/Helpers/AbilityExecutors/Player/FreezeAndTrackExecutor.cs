using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FreezeAndTrackExecutor :ExecutorBase
{
    private IFreezeAndDeflectable[] _arr = new IFreezeAndDeflectable[64];
    private int _count;


    public override void Execute(in AbilityContext context, EffectDef def, CuePhase phase)
    {
        if (def.Kind != EffectKind.FreezeAndTrack && def.Kind != EffectKind.ReturnTrackedOnEnd) return;

        if(phase == CuePhase.Start || phase == CuePhase.Impact) Add(in context);
        else ClearAndFireBack(); 

    }



    private void Add(in AbilityContext context)
    {
        for (int i = 0; i < context.TargetCount; i++)
        {
            var go = context.Targets[i].gameObject;
            if (ComponentRegistry.TryGet(go, out IFreezeAndDeflectable freezeable))
            {
                if (CanFreeze(freezeable)) freezeable.Freeze();
            }
        }
    }

    private bool CanFreeze(IFreezeAndDeflectable freezable)
    {
        for (int i = 0; i < _count; i++) if (ReferenceEquals(_arr[i], freezable)) return false;
        if (_count == _arr.Length) Array.Resize(ref _arr, _arr.Length * 2);
        _arr[_count++] = freezable;
        return true;

    }

    private void ClearAndFireBack()
    {
        if (_arr == null || _arr.Length == 0) return;

        CoroutineRunner.Instance.StartCoroutine(FireFreezablesDelay());
    }

    private IEnumerator FireFreezablesDelay()
    {
      
        for (int i = 0; i < _arr.Length; i++)
        {
            var d = _arr[i];
            if (d != null)
            {
                d.Deflect(ProjectileKickType.ReFire);
                yield return new WaitForSeconds(0.1f);
            }

        }
        Array.Clear(_arr, 0, _count);
        _count = 0;
    }
}

[Obsolete]
public sealed class ReturnTrackedOnEndExecutor : IEffectExecutor
{
    private readonly TrackedFreezeables _tracker;

    public ReturnTrackedOnEndExecutor(TrackedFreezeables tracker) => _tracker = tracker;

    public bool IsReady => throw new NotImplementedException();

    public void Execute(in AbilityContext context, EffectDef def, CuePhase phase)
    {
        if (phase != CuePhase.End || def.Kind != EffectKind.ReturnTrackedOnEnd) return;
        _tracker?.ClearFreezeables();
    }

    public void UpdatePool(PoolIdSO poolId, Action<bool> actionCompleteCallback)
    {
        throw new NotImplementedException();
    }
}
