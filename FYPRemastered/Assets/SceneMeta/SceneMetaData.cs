using System;
using UnityEngine;

[Serializable]
public class SceneMetaData
{
    public FsmFeatureGroup FsmFeatures;
}

[Serializable]
public class FsmFeatureGroup
{
    public bool UsedInScene;

    public HumanoidFsmFeature Humanoid;

 /*   public FeatureMeta SpeedData;
    public FeatureMeta Waypoints;
    public FeatureMeta FlankPoints;
    public FeatureMeta PatrolData;
    public FeatureMeta ChaseData;*/
}


[Serializable]
public class HumanoidFsmFeature : FsmFeatureBase
{
    public bool UsedInScene;

    public FeatureMeta SpeedData;
    public FeatureMeta Waypoints;
    public FeatureMeta FlankPoints;
    public FeatureMeta PatrolData;
    public FeatureMeta ChaseData;
}

[Serializable]
public abstract class FsmFeatureBase { }