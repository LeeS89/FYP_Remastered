using UnityEngine;

public enum AIResourceType
{
    None,
    WaypointBlock,
    FlankPointCandidates,
    FlankPointEvaluationMasks
   
}

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


