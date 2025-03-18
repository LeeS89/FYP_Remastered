using UnityEngine;

[CreateAssetMenu(fileName = "DamageData", menuName = "Scriptable Objects/DamageData")]
public class DamageData : ScriptableObject
{
    public DamageType damageType;  
    public float baseDamage;       
    public float damageOverTime;   
    public float duration;
}
