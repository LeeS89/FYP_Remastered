using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDefinition", menuName = "Abilities/AbilityDefinition")]
public class AbilityDefinition : ScriptableObject
{
    [SerializeField] public List<AbilityBase> bases;
    [SerializeField] public AbilityBase Ability;
}
