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

    public FeatureMeta Waypoints;
    public FeatureMeta FlankPoints;
    public FeatureMeta PatrolData;
    public FeatureMeta ChaseData;
}