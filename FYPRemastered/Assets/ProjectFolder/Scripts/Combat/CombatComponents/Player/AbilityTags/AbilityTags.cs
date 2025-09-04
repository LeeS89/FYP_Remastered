using UnityEngine;

[CreateAssetMenu(fileName = "AbilityTags", menuName = "Abilities/Tag")]
public class AbilityTags : ScriptableObject
{
    [TextArea] public string Description;
}
