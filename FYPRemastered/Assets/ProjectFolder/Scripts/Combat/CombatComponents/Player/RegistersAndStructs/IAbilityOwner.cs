using UnityEngine;

public interface IAbilityOwner
{
    Transform ExecuteOrigin { get; }
    Transform DirectionOrigin { get; }
    float DirectionOffset { get; }
    bool HasTag(AbilityTags tag);
    void AddTag(AbilityTags tag);
    void RemoveTag(AbilityTags tag);
   

    bool HasSufficientResources(ResourceCost cost);

    void Spend(ResourceCost cost);

    void PlayCue(CueDef cue);
}
