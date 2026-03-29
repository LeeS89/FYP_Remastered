using System;
using UnityEngine;

[Obsolete("", true)]
public enum AIResourceType
{
    None,
    WaypointBlock,
    FlankPointCandidates,
    FlankPointEvaluationMasks,
    Path
   
}

[Obsolete("", true)]
public enum AIDestinationType
{
    None,
    ChaseDestination,
    FlankDestination,
    PatrolDestination
} 

public enum PoolResourceType
{
    None,
    NormalBulletPool,
    PoisionBulletPool,
    DeflectAudioPool,
    BasicHitParticlePool
}


