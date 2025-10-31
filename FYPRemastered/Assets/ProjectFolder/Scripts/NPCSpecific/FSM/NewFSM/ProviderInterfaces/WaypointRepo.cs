using System;
using UnityEngine;

public sealed class WaypointRepo : IWaypointRepository
{
    public static readonly WaypointRepo Instance = new();
    private WaypointRepo() { }


    public void GetWaypointBlock(Action<BlockData> requestCallback)
     => this.RequestWaypointBlock(requestCallback);

    public void SwitchWaypointBlock(BlockData oldBlock, Action<BlockData> requestCallback)
    {
        throw new NotImplementedException();
    }
}


