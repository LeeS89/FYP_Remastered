
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceRequest
{
  
    public Resourcetype resourceType { get; set; }

    public BlockData blockData { get; set; }
    public Action<BlockData> waypointCallback { get; set; }


    public int numSteps { get; set; }
    public Action<List<Vector3>> FlankPointCandidatesCallback { get; set; }

    public Action<Bullet> bulletCallback { get; set; }
}
