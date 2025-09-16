using UnityEngine;

[CreateAssetMenu(fileName = "AbilityTags", menuName = "Abilities/Tag")]
public class AbilityTags : ScriptableObject
{
    [TextArea] public string Description;
    [SerializeField] private string _id;

    public string Id => _id;
}
