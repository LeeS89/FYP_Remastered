using UnityEngine;

public interface IFsmPatrolData : IFsmData, IFsmStateData
{
    float GetIdleTimeSeconds();
}
