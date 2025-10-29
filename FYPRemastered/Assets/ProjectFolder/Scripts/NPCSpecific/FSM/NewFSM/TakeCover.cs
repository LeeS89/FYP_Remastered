using UnityEngine;

public sealed class TakeCover : IntentStateBase
{
    public static readonly TakeCover Instance = new();
    private TakeCover() { }

    public override void Enter(NPCController self)
    {
        throw new System.NotImplementedException();
    }
}
