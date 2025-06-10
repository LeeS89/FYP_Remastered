
using System;
using UnityEngine;

public class ResourceRequest
{
    //public ResourceType ResourceType { get; set; }
    public Resourcetype resourceType { get; set; }
    public Action<BlockData> waypointCallback { get; set; }
    public Action<Bullet> bulletCallback { get; set; }
}
