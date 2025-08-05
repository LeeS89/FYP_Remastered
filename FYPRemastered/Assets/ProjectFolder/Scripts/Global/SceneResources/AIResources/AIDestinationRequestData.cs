using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This class acts as a middle man betweeen AI requesting potential destinations, and Resource classes providing the necessary data via callbacks
/// to functions from the sending class
/// </summary>
public class AIDestinationRequestData : ProjectileResourceRequest
{
    public AIResourceType resourceType { get; set; }

    public BlockData blockData { get; set; }
    public Action<BlockData> waypointCallback { get; set; }


    public int numSteps { get; set; }
    public Action</*List<Vector3>*/bool> FlankPointCandidatesCallback { get; set; }
    public LayerMask flankTargetMask { get; set; }
    public LayerMask flankBlockingMask { get; set; }
    public Collider[] flankTargetColliders;

    public AIDestinationType destinationType;
    public Vector3 start;
    public Vector3 end;
    public NavMeshPath path;
    public NavMeshObstacle obstacle;

    public Action<bool> internalCallback; // Callback for internal use, e.g., within the same system or component
    public Action<bool, Vector3> externalCallback; // Callback for external use, e.g., to notify original caller
    public Action carvingCallback; // Callback for carving operations, if needed
    public Action agentActiveCallback;

    public List<Vector3> flankPointCandidates; // OLD
    public List<FlankPointData> flankCandidates; // NEW
  

    public override void OnInstanceDestroyed()
    {
        base.OnInstanceDestroyed();
       
        flankCandidates = null; // NEW
        blockData = null;
        internalCallback = null;
        externalCallback = null;
        waypointCallback = null;
        FlankPointCandidatesCallback = null;
        path = null;
        obstacle = null;
        carvingCallback = null;
        agentActiveCallback = null;
        flankTargetColliders = null;

    }
}
