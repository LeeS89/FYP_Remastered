using System;
using UnityEngine;

[Obsolete]
public sealed class TakeCoverObsolete : IntentStateBase
{
    public static readonly TakeCoverObsolete Instance = new();
    private TakeCoverObsolete() { }

    public override void Enter(IFSMOwner self)
    {
        throw new System.NotImplementedException();
    }
}
