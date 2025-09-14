using UnityEngine;

public interface IAbilityOwner
{
    Transform Origin { get; }
    Transform GazeOrigin { get; }
    bool HasTag(AbilityTags tag);
    void AddTag(AbilityTags tag);
    void RemoveTag(AbilityTags tag);
   

    bool HasSufficientResources(ResourceCost[] costs);

    void SpendAll(ResourceCost[] costs);

    void PlayCue(CueDef cue);
}
