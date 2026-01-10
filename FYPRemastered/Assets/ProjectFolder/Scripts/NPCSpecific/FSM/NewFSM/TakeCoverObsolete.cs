using System;
using UnityEngine;

[Obsolete]
public sealed class TakeCoverObsolete : IntentStateBaseObsolete
{
    public static readonly TakeCoverObsolete Instance = new();
    private TakeCoverObsolete() { }

    public override void Enter(IFSMOwner self)
    {
        throw new System.NotImplementedException();
    }
}
